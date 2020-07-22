using Assets.Server.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assets.Server.Protocol;
using UnityEngine.UI;
using System;
using System.Linq;
using MyFirstServ.Models.TouchItFaster;
using Assets.Scripts.Multiplayer_Scene;
using Assets.Scripts.Preloader;
using Assets.Scripts.Utils;

public class MultiplayerManager : MonoBehaviour {

    public GameObject FriendGO;
    public GameObject FriendsContainer;
    public GameObject OnFriendClickPanel;
    public Button FightButton;
    public Button UnfriendButton;
    public Button AddfriendButton;
    public Text NameText;
    public Text HighestScoreText;
    public Text WinsText;
    public Text LosesText;
    public Button QuitButton;
    public Text ChallengeRequestText;
    public GameObject MessagePanel;
    public GameObject ChallengRequestPanel;
    public GameObject AcceptButton;
    public GameObject DeclineButton;

    //Stats panel
    public GameObject PlayerNameText;
    public GameObject PlayerHighestScoreText;
    public GameObject PlayerWinsText;
    public GameObject PlayerLosesText;
    public GameObject PlayerTapsText;

    private List<PlayerInfo> Friends = new List<PlayerInfo>();
    private List<GameObject> FriendsGO = new List<GameObject>();

    private List<PlayerInfo> SearchedUsers = new List<PlayerInfo>();
    private List<GameObject> SearchedGO = new List<GameObject>();

    private bool FriendsUpdated = false, SearchedPlayers = false;


	void Start () {
        UpdateFriends();
        BuildStatsPanel();
        ServerManager.Instance.Client.AddCallback(URI.ChallengeReply, ChallengeRequestReply);
	    ServerManager.Instance.Client.AddCallback(URI.ChallengeRequest, OnFriendsChallengeRequest);
    }

    private void BuildStatsPanel()
    {
        PlayerNameText.GetComponent<Text>().text = PlayerContainer.Instance.Info.PlayerName;
        PlayerHighestScoreText.GetComponent<Text>().text = "" + PlayerContainer.Instance.Info.MultiplayerHighestScore;
        PlayerWinsText.GetComponent<Text>().text = "" + PlayerContainer.Instance.Info.Wins + " wins";
        PlayerLosesText.GetComponent<Text>().text = "" + PlayerContainer.Instance.Info.Loses + " loses";
        PlayerTapsText.GetComponent<Text>().text = "" + PlayerContainer.Instance.Info.MaxHitsInRowMultiplayer + " taps in a row";
    }

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
        ClearFriendsList();
        ClearSearchedPlayersList();

        foreach (PlayerInfo f in Friends.ToArray())
        {
            GameObject go = Instantiate(FriendGO, FriendsContainer.transform) as GameObject;
            go.GetComponentInChildren<Text>().text = f.PlayerName;
            Button b = go.GetComponent<Button>();

            string playerState = f.CurrentState == PlayerState.Offline ? "OfflineImage" : "OnlineImage";
            go.transform.Find(playerState).gameObject.SetActive(true);

            b.onClick.AddListener(() =>
            {
                FillProfilePanel(f, true);
            });

            FriendsGO.Add(go);
        }
    }

    private void ClearFriendsList()
    {
        foreach (GameObject go in FriendsGO)
        {
            Destroy(go);
        }

        FriendsGO.Clear();
    }

    private void SearchPlayersUpdate()
    {
        SearchedPlayers = false;
        ClearFriendsList();
        ClearSearchedPlayersList();
        //NoResults.SetActive(false);

        if (SearchedUsers.Count == 0)
        {
            //NoResults.SetActive(true);
        }
        else
        {
            foreach (PlayerInfo pi in SearchedUsers.ToArray())
            {
                GameObject go = Instantiate(FriendGO, FriendsContainer.transform) as GameObject;
                go.GetComponentInChildren<Text>().text = pi.PlayerName;
                SearchedGO.Add(go);
                Button b = go.GetComponent<Button>();

                string playerState = pi.CurrentState == PlayerState.Offline ? "OfflineImage" : "OnlineImage";
                go.transform.Find(playerState).gameObject.SetActive(true);

                b.onClick.AddListener(() =>
                {
                    FillProfilePanel(pi, false);
                });
            }
        }
    }

    private void ClearSearchedPlayersList()
    {
        foreach (GameObject go in SearchedGO)
        {
            Destroy(go);
        }

        SearchedGO.Clear();
    }

    private void FillProfilePanel(PlayerInfo f, bool friend)
    {
        NameText.text = f.PlayerName;
        HighestScoreText.text = f.MultiplayerHighestScore.ToString();
        WinsText.text = f.Wins.ToString() + " wins";
        LosesText.text = f.Loses.ToString() + " loses";
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
                UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("Let's see if he have enough courage...", 3f, MessagePanel)));
            }
            else if (reply.Reply == ChallengeReplyType.ChallengeAccepted)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("You got a challenge!", 0.5f, MessagePanel)));
                GameContainer.CurrentGameId = reply.ChallengeID;
                ServerManager.Instance.NextScene = "MultiplayerInGame";
            }
            else if (reply.Reply == ChallengeReplyType.ChallengeRefused)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("Ahah he's scared as sh*t", 3f, MessagePanel)));
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
        if(value.Length >= 3)
        {
            ServerManager.Instance.Client.Send(URI.SearchPlayer, value, PlayerSearchResult);
        }
        else
        {
            FriendsUpdated = true;
        }
    }

    private void PlayerSearchResult(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            SearchedUsers.Clear();
            ArrayWrapper mi = p.DeserializeContent<ArrayWrapper>();
            SearchedUsers.AddRange(mi.GetArray<PlayerInfo>().OrderBy(pi => pi.PlayerName));
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
            FightButton.gameObject.SetActive(f.CurrentState == PlayerState.Online);
            UnfriendButton.gameObject.SetActive(true);
            FightButton.onClick.RemoveAllListeners();
            FightButton.onClick.AddListener(() =>
            {
                ChallengeRequest(f.ID);
            });
            UnfriendButton.onClick.RemoveAllListeners();
            UnfriendButton.onClick.AddListener(() =>
            {
                ServerManager.Instance.Client.Send(URI.Unfriend, f.ClientID, UnfriendReply);
                OnFriendClickPanel.SetActive(false);
            });
        }
        else
        {
            AddfriendButton.gameObject.SetActive(true);
            FightButton.gameObject.SetActive(f.CurrentState == PlayerState.Online);
            UnfriendButton.gameObject.SetActive(false);
            AddfriendButton.onClick.RemoveAllListeners();
            AddfriendButton.onClick.AddListener(() =>
            {
                ServerManager.Instance.Client.Send(URI.AddFriend, f.ClientID, AddFriendReply);
                OnFriendClickPanel.SetActive(false);
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
            UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("Ye that boy was kicked out!", 3f, MessagePanel)));
        }
        else
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("Couldn't delete user.", 3f, MessagePanel)));
        }
    }

    private void AddFriendReply(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            PlayerInfo added = p.DeserializeContent<PlayerInfo>();
            Friends.Add(added);
            FriendsUpdated = true;
            UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("Uuuuuh so needy.", 2f, MessagePanel)));
        }
        else
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("Couldn't add user.", 2f, MessagePanel)));
        }

    }

    public void RandomChallenge()
    {
        ServerManager.Instance.Client.Send(URI.RandomChallengeRequest, null, OnRandomChallenge);
        StartCoroutine(UIUtils.ShowMessageInPanel("Searching players...", 10f, MessagePanel));
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

    private void OnFriendsChallengeRequest(JsonPacket p)
    {
        Debug.Log("Multiplayer Menu: Friend's challenge request received");
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            ChallengeRequest cr = p.DeserializeContent<ChallengeRequest>();
            BuildChallengeRequestPanel(cr);
        }
    }

    private void BuildChallengeRequestPanel(ChallengeRequest cr)
    {
        PlayerInfo opponent = Friends.FirstOrDefault(f => f.ID == cr.RequesterID);

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            ChallengeRequestText.text = ChallengeRequestText.text.Replace("NAME", opponent.PlayerName);
            AcceptButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log("Multiplayer Menu: Accepting friend's challenge");
                ChallengeReply creply = new ChallengeReply()
                {
                    ChallengeID = cr.ID,
                    Reply = ChallengeReplyType.ChallengeAccepted
                };
                ServerManager.Instance.Client.Send(URI.ChallengeReply, creply);

                UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("You got a challenge!", 0.5f, MessagePanel)));
                GameContainer.CurrentGameId = creply.ChallengeID;
                ServerManager.Instance.NextScene = "MultiplayerInGame";

            });
            DeclineButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log("Multiplayer Menu: Declining friend's challenge");
                ChallengeReply creply = new ChallengeReply()
                {
                    ChallengeID = cr.ID,
                    Reply = ChallengeReplyType.ChallengeRefused
                };
                ServerManager.Instance.Client.Send(URI.ChallengeReply, creply);
            });
            ChallengRequestPanel.SetActive(true);
        });
        
    }
}
