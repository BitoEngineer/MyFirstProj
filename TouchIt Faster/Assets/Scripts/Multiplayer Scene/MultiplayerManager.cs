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

public class MultiplayerManager : MonoBehaviour {

    public GameObject FriendGO;
    public GameObject FriendsContainer;
    public GameObject SearchContainer;
    public GameObject OnFriendClickPanel;
    public Button FightButton;
    public Button UnfriendButton;
    public Text NameText;
    public Text HighestScoreText;
    public Text WinsText;
    public Text LosesText;
    public Text TotalTimeText;
    public Button QuitButton;

    private List<PlayerInfo> Friends = new List<PlayerInfo>();
    private List<GameObject> FriendsGO = new List<GameObject>();
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
            FriendsUpdated = false;
            foreach(GameObject go in FriendsGO)
            {
                Destroy(go);
            }

            FriendsGO.Clear();

            foreach(PlayerInfo f in Friends.ToArray())
            {
                GameObject go=Instantiate(FriendGO, FriendsContainer.transform) as GameObject;
                go.GetComponentInChildren<Text>().text = f.PlayerName;
                Button b=go.GetComponent<Button>();

                Color32 color;
                if(f.CurrentState == PlayerState.Offline)
                {
                     color = new Color32(170, 36, 41, 255);
                }
                else
                {
                    color = new Color32(36, 129, 41, 255);               
                }
                b.GetComponent<Image>().color = color;

                b.onClick.AddListener(() =>
                {
                    FillProfilePanel(f);
                });

                FriendsGO.Add(go);
            }
        }

        if (SearchedPlayers)
        {
            SearchedPlayers = false;

            foreach (GameObject go in SearchedGO)
            {
                Destroy(go);
            }

            SearchedGO.Clear();

            /*foreach (PlayerInfo pi in ps.Result)
            {
                GameObject go = Instantiate(FriendGO, SearchContainer.transform) as GameObject;
                go.GetComponentInChildren<Text>().text = pi.PlayerName;
                SearchedGO.Add(go);
            }*/
        }
	}

    private void FillProfilePanel(PlayerInfo f)
    {
        NameText.text = f.PlayerName;
        HighestScoreText.text = "Highest Score: "+f.HighestScore.ToString();
        WinsText.text= "Wins: "+f.Wins.ToString();
        LosesText.text = "Loses: "+f.Loses.ToString();

        FightButton.onClick.AddListener(() =>
        {
            ChallengeRequest(f.ID);
        });

        QuitButton.onClick.AddListener(() =>
        {
            OnFriendClickPanel.SetActive(false);
        });

        UnfriendButton.onClick.AddListener(() =>
        {
            /*TODO*/
        });

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
        ServerManager.Instance.Client.Send(URI.SearchPlayer, new PlayerSearch() { Key = value }, PlayerSearchResult);
    }


    private void PlayerSearchResult(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            p.DeserializeContent<PlayerSearch>();
            SearchedPlayers = true;
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
}
