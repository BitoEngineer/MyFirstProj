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
using GooglePlayGames.BasicApi.Multiplayer;
using System.Net.Mime;
using Assets.Server.DTO;

public class MultiplayerManager : MonoBehaviour
{

    public GameObject RandomChallengeButton;
    public GameObject NoConnectionImage;
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
    public GameObject NumPlayersOnlineText;

    //Stats panel
    public GameObject PlayerHighestScoreText;
    public GameObject PlayerWinsText;
    public GameObject PlayerLosesText;
    public GameObject PlayerTapsText;
    public GameObject NameLayoutGroup;

    private Transform PlayerNameText => NameLayoutGroup.transform.Find("PlayerNameText");
    private Transform EditNameButton => NameLayoutGroup.transform.Find("EditNameButton");
    private Transform ChangeNameInputField => NameLayoutGroup.transform.Find("ChangeNameInputField");

    private List<PlayerInfo> Friends = new List<PlayerInfo>();
    private List<GameObject> FriendsGO = new List<GameObject>();

    private List<PlayerInfo> SearchedUsers = new List<PlayerInfo>();
    private List<GameObject> SearchedGO = new List<GameObject>();

    private bool FriendsUpdated = false, SearchedPlayers = false;
    private DateTime LastGetPlayersOnlineRequest { get; set; } = DateTime.UtcNow;

    void Start()
    {
        if (ConnectionHelper.HasInternet)
        {
            UpdateFriends();
        }
        else
        {
            SetOfflineMode();
        }

        BuildStatsPanel();
        ServerManager.Instance.Client.AddCallback(URI.ChallengeRequest, OnFriendsChallengeRequest);
        ServerManager.Instance.Client.AddCallback(URI.ChallengeReply, OnChallengeReply);
        ServerManager.Instance.Client.AddCallback(URI.FriendConnectivityChanged, OnFriendConnectivityChanged);
    }

    private void SetOfflineMode()
    {
        RandomChallengeButton.GetComponent<Button>().interactable = false;
        NoConnectionImage.SetActive(true);
    }

    private void BuildStatsPanel()
    {
        ChangeNameInputField.gameObject.SetActive(false);
        PlayerNameText.gameObject.SetActive(true);
        EditNameButton.gameObject.SetActive(true);

        PlayerNameText.GetComponent<Text>().text = PlayerContainer.Instance.Info.PlayerName;
        PlayerHighestScoreText.GetComponent<Text>().text = "" + PlayerContainer.Instance.Info.MultiplayerHighestScore;
        PlayerWinsText.GetComponent<Text>().text = "" + PlayerContainer.Instance.Info.Wins + " wins";
        PlayerLosesText.GetComponent<Text>().text = "" + PlayerContainer.Instance.Info.Loses + " loses";
        PlayerTapsText.GetComponent<Text>().text = "" + PlayerContainer.Instance.Info.MaxHitsInRowMultiplayer + " taps in a row";
    }

    void Update()
    {
        if (FriendsUpdated)
        {
            FriendsContainerUpdate();
        }

        if (SearchedPlayers)
        {
            SearchPlayersUpdate();
        }

        if ((DateTime.UtcNow - LastGetPlayersOnlineRequest).TotalSeconds >= 4)
        {
            try
            {
                SendGetPlayersOnline();
            }
            finally
            {
                LastGetPlayersOnlineRequest = DateTime.UtcNow;
            }
        }
    }

    private void FriendsContainerUpdate()
    {
        FriendsUpdated = false;
        ClearFriendsList();
        ClearSearchedPlayersList();

        foreach (PlayerInfo f in Friends.OrderBy(u => u.PlayerName).ToArray())
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
            foreach (PlayerInfo pi in SearchedUsers.Where(u => u.ID != PlayerContainer.Instance.Info.ID).ToArray())
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

    private void OnChallengeReply(JsonPacket p)
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
                GameContainer.SetChallengeId(reply.ChallengeID);
                ServerManager.Instance.NextScene = "MultiplayerInGame";
            }
            else if (reply.Reply == ChallengeReplyType.ChallengeRefused)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("Ahah he's scared as sh*t", 3f, MessagePanel)));
            }
        }
        else if (p.ReplyStatus == ReplyStatus.Conflict)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("Ooops, opponent left or is already in game", 2f, MessagePanel)));
        }
        else
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("Some problem occurred", 2f, MessagePanel)));
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

    private void SendGetPlayersOnline()
    {
        if (ConnectionHelper.HasInternet)
        {
            ServerManager.Instance.Client.Send(URI.GetOnlinePlayers, null, (p) =>
            {
                if (p.ReplyStatus == ReplyStatus.OK)
                {
                    var dto = p.DeserializeContent<GetPlayersOnlineDto>();
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        NumPlayersOnlineText.GetComponent<Text>().text = dto.NumPlayers + " players";
                    });
                }
            });
        }
    }

    public void SearchPlayers(string value)
    {
        if (!ConnectionHelper.HasInternet)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("You need to connect bro", 3f, MessagePanel)));
            return;
        }

        if (value.Length >= 3)
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
        ServerManager.Instance.Client.Send(URI.ChallengeRequest, cr, OnChallengeReply);
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
                ServerManager.Instance.Client.Send(URI.Unfriend, f.ClientID, OnUnfriend);
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
                ServerManager.Instance.Client.Send(URI.AddFriend, f.ClientID, OnAddFriend);
                OnFriendClickPanel.SetActive(false);
            });
        }
    }

    private void OnUnfriend(JsonPacket p)
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

    private void OnAddFriend(JsonPacket p)
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

    private void OnFriendConnectivityChanged(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            PlayerInfo pi = p.DeserializeContent<PlayerInfo>();

            Friends.FirstOrDefault(f => f.ID == pi.ID).CurrentState = pi.CurrentState;

            FriendsUpdated = true;
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
                GameContainer.StartGame(reply.ChallengeID, reply.OpponentName, reply.OpponentID);
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
                ServerManager.Instance.Client.Send(URI.ChallengeReply, creply, OnChallengeAcceptedOkCallback, new ReplyStatus[] { ReplyStatus.OK }, OnChallengeAccepteNotOkCallback);
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

                UnityMainThreadDispatcher.Instance().Enqueue(() => ChallengRequestPanel.SetActive(false));
            });
            ChallengRequestPanel.SetActive(true);
        });

    }

    public void OnChallengeAccepteNotOkCallback(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.Conflict)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                ChallengRequestPanel.SetActive(false);
                StartCoroutine(UIUtils.ShowMessageInPanel("Ooops, opponent left or is already in game", 2f, MessagePanel));
            });
        }
        else
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                ChallengRequestPanel.SetActive(false);
                StartCoroutine(UIUtils.ShowMessageInPanel("Some problem occurred", 2f, MessagePanel));
            });
        }
    }

    public void OnChallengeAcceptedOkCallback(JsonPacket p)
    {
        var creply = p.DeserializeContent<ChallengeReply>();
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            StartCoroutine(UIUtils.ShowMessageInPanel("You got a challenge!", 0.5f, MessagePanel));
        });
        GameContainer.SetChallengeId(creply.ChallengeID);
        ServerManager.Instance.NextScene = "MultiplayerInGame";
    }

    public void EditName_OnValueChanged()
    {
        var value = ChangeNameInputField.Find("Text").GetComponent<Text>().text;
        if (value.Length > 15)
        {
            var textGo = ChangeNameInputField.transform.Find("Text").GetComponent<Text>();
            textGo.text = textGo.text.Substring(0, 15);

            UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("Name can't have more than 15 chars", 3f, MessagePanel)));
        }
    }

    public void EditName_OnValueEnd()
    {
        if (!ConnectionHelper.HasInternet)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("You need to connect bro", 3f, MessagePanel)));
            return;
        }
            
        var value = ChangeNameInputField.Find("Text").GetComponent<Text>().text;
        if (value.Length > 15)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("Name can't have more than 15 chars", 3f, MessagePanel)));
            return;
        }

        if (value.Length < 3)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("Name can't have less than 3 chars", 3f, MessagePanel)));
            });
            return;
        }

        if (value.Length == 0)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                BuildStatsPanel();
            });
            return;
        }

        ServerManager.Instance.Client.Send(URI.ChangeName, new ChangeNameDto() { Name = value }, OnNameChangedCallback);
    }

    private void OnNameChangedCallback(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.Conflict)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("Name already exists, stop imitating boy", 3f, MessagePanel)));
        }
        else if (p.ReplyStatus == ReplyStatus.OK)
        {
            var pInfo = p.DeserializeContent<PlayerInfo>();
            PlayerContainer.Instance.SetPlayerInfo(pInfo);

            UnityMainThreadDispatcher.Instance().Enqueue((Action)(() =>
            {
                BuildStatsPanel();
            }));
        }
    }
}
