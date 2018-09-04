using Assets.Server.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assets.Server.Protocol;
using UnityEngine.UI;
using System;
using MyFirstServ.Models.TouchItFaster;
using Assets.Scripts.Multiplayer_Scene;
using Assets.Scripts.Preloader;

public class MultiplayerManager : MonoBehaviour {

    public GameObject FriendGO;
    public GameObject FriendsContainer;
    public GameObject SearchContainer;
    public GameObject OnFriendClickPanel;
    public Button FightButton;
    public Button UnfriendButton;
    public Button AddfriendButton;
    public Text NameText;
    public Text HighestScoreText;
    public Text WinsText;
    public Text LosesText;
    public Text TotalTimeText;
    public Button QuitButton;
    public GameObject NoResults;
    public GameObject MessagePanel;

    private Color32 Red = new Color32(170, 36, 41, 255);
    private Color32 Green = new Color32(36, 129, 41, 255);

    private List<PlayerInfo> Friends = new List<PlayerInfo>();
    private List<GameObject> FriendsGO = new List<GameObject>();

    private List<PlayerInfo> SearchedUsers = new List<PlayerInfo>();
    private List<GameObject> SearchedGO = new List<GameObject>();

    private bool FriendsUpdated = false, SearchedPlayers = false;

	// Use this for initialization
	void Start () {
        UpdateFriends();
        ServerManager.Instance.Client.AddCallback(URI.ChallengeReply, ChallengeRequestReply);
    }


    // Update is called once per frame
    void Update () {
        if (FriendsUpdated)
        {
            FriendsContainerUpdate();
        }

        if (SearchedPlayers)
        {
            SearchPlayersUpdate();
        }
    }

    private void FriendsContainerUpdate()
    {
        FriendsUpdated = false;
        foreach (GameObject go in FriendsGO)
        {
            Destroy(go);
        }

        FriendsGO.Clear();

        foreach (PlayerInfo f in Friends.ToArray())
        {
            GameObject go = Instantiate(FriendGO, FriendsContainer.transform) as GameObject;
            go.GetComponentInChildren<Text>().text = f.PlayerName;
            Button b = go.GetComponent<Button>();

            b.GetComponent<Image>().color = f.CurrentState == PlayerState.Offline ? Red : Green;

            b.onClick.AddListener(() =>
            {
                FillProfilePanel(f, true);
            });

            FriendsGO.Add(go);
        }
    }

    private void SearchPlayersUpdate()
    {
        SearchedPlayers = false;
        NoResults.SetActive(false);

        foreach (GameObject go in SearchedGO)
        {
            Destroy(go);
        }

        SearchedGO.Clear();

        if (SearchedUsers.Count == 0)
        {
            NoResults.SetActive(true);
        }
        else
        {
            foreach (PlayerInfo pi in SearchedUsers.ToArray())
            {
                GameObject go = Instantiate(FriendGO, SearchContainer.transform) as GameObject;
                go.GetComponentInChildren<Text>().text = pi.PlayerName;
                SearchedGO.Add(go);
                Button b = go.GetComponent<Button>();

                b.GetComponent<Image>().color = pi.CurrentState == PlayerState.Offline ? Red : Green;

                b.onClick.AddListener(() =>
                {
                    FillProfilePanel(pi, false);
                });
            }
        }
    }

    private void FillProfilePanel(PlayerInfo f, bool friend)
    {
        NameText.text = f.PlayerName;
        HighestScoreText.text = "Highest Score: " + f.HighestScore.ToString();
        WinsText.text = "Wins: " + f.Wins.ToString();
        LosesText.text = "Loses: " + f.Loses.ToString();
        BuildProfilePanelButtons(f, friend);

        QuitButton.onClick.AddListener(() =>
        {
            OnFriendClickPanel.SetActive(false);
        });

        OnFriendClickPanel.transform.position = Input.mousePosition;

        OnFriendClickPanel.SetActive(true);
    }

    

    private void ChallengeRequestReply(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            var reply = p.DeserializeContent<ChallengeReply>();

            if (reply.Reply == ChallengeReplyType.Waiting)
            {
                /*TODO Show txt*/
            }
            else if (reply.Reply == ChallengeReplyType.ChallengeAccepted)
            {
                GameContainer.CurrentGameId = reply.ChallengeID;
                ServerManager.Instance.NextScene = "MultiplayerInGame";
            }
        }
        else if (p.ReplyStatus == ReplyStatus.Forbidden)
        {
            /*TODO*/
        }
    }

    public void UpdateFriends()
    {
        ServerManager.Instance.Client.Send(URI.Friends, null, RequestFriendsNamesReply);
    }

    private void RequestFriendsNamesReply(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            Friends.Clear();
            try
            {
                ArrayWrapper mi = p.DeserializeContent<ArrayWrapper>();
                Friends.AddRange(mi.GetArray<PlayerInfo>());
                FriendsUpdated = true;
            }
            catch (Exception e)
            {
                Debug.Log("Fódeu gerau");
            }

        }
    }

    public void SearchPlayers(string value)
    {
        ServerManager.Instance.Client.Send(URI.SearchPlayer, value, PlayerSearchResult);
    }


    private void PlayerSearchResult(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            SearchedUsers.Clear();
            ArrayWrapper mi = p.DeserializeContent<ArrayWrapper>();
            SearchedUsers.AddRange(mi.GetArray<PlayerInfo>());
            SearchedPlayers = true;
        }
        else
        {
            /*TODO*/
        }
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void ChallengeRequest(long requestedId)
    {
        ChallengeRequest cr = new ChallengeRequest()
        {
            RequesterID = PlayerContainer.Instance.Info.ID,
            RequestedID = requestedId
        };
        ServerManager.Instance.Client.Send(URI.ChallengeRequest, cr, ChallengeRequestReply);
    }

    private void BuildProfilePanelButtons(PlayerInfo f, bool friend)
    {
        if (friend)
        {
            AddfriendButton.gameObject.SetActive(false);
            FightButton.gameObject.SetActive(true);
            UnfriendButton.gameObject.SetActive(true);
            FightButton.onClick.AddListener(() =>
            {
                ChallengeRequest(f.ID);
            });

            UnfriendButton.onClick.AddListener(() =>
            {
                ServerManager.Instance.Client.Send(URI.Unfriend, f.ClientID, UnfriendReply);
            });
        }
        else
        {
            AddfriendButton.gameObject.SetActive(true);
            FightButton.gameObject.SetActive(false);
            UnfriendButton.gameObject.SetActive(false);
            AddfriendButton.onClick.AddListener(() =>
            {
                ServerManager.Instance.Client.Send(URI.AddFriend, f.ClientID, AddFriendReply);
            });
        }
    }

    private void UnfriendReply(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            PlayerInfo removed = p.DeserializeContent<PlayerInfo>();
            Friends.RemoveAll(pl => pl.ClientID.Equals(removed.ClientID));
            FriendsUpdated = true;
            UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(ShowMessage("Ye that boy was kicked out!", 3f)));
        }
        else
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(ShowMessage("Couldn't delete user.", 3f)));
        }
    }

    private void AddFriendReply(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            PlayerInfo added = p.DeserializeContent<PlayerInfo>();
            Friends.Add(added);
            FriendsUpdated = true;
            UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(ShowMessage("Uuuuuh so needy.", 2f)));
        }
        else
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(ShowMessage("Couldn't add user.", 2f)));
        }

    }

    public void RandomChallenge()
    {
        ServerManager.Instance.Client.Send(URI.RandomChallengeRequest, null, OnRandomChallenge);
        StartCoroutine(ShowMessage("Searching players...", 10f));
    }

    private void OnRandomChallenge(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            var reply = p.DeserializeContent<ChallengeReply>();

            if (reply.Reply == ChallengeReplyType.Waiting)
            {
                /*TODO Show txt*/
            }
            else if (reply.Reply == ChallengeReplyType.ChallengeAccepted)
            {
                GameContainer.CurrentGameId = reply.ChallengeID;
                ServerManager.Instance.NextScene = "MultiplayerInGame";
            }
        }
        else if (p.ReplyStatus == ReplyStatus.Forbidden)
        {
            /*TODO*/
        }
    }

    IEnumerator ShowMessage(string message, float delay)
    {
        MessagePanel.SetActive(true);
        MessagePanel.GetComponentInChildren<Text>().text = message;
        yield return new WaitForSeconds(delay);
        MessagePanel.SetActive(false);
    }
}
