using Assets.Scripts.Preloader;
using Assets.Scripts.Utils;
using Assets.Server.Models;
using Assets.Server.Protocol;
using MyFirstServ.Models.TouchItFaster;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SinglePlayerMenuManager : MonoBehaviour
{

    public GameObject HghestScoreText;
    public GameObject MaxTapsText;
    public GameObject TopPlayersList;
    public GameObject NoConnectionImage;

    public GameObject MessagePanel;

    public GameObject UserGO;
    public GameObject UserContainer;
    public GameObject UserProfilePanel;

    private Text UserProfilePanel_Name => UserProfilePanel.transform.Find("NameAndQuitLayoutGroup").transform.Find("NameText").GetComponent<Text>();
    private Text UserProfilePanel_HighestScore => UserProfilePanel.transform.Find("StatsLayoutGroup").transform.Find("HighestScorePanel").transform.Find("HighestScoreText").GetComponent<Text>();
    private Text UserProfilePanel_Taps => UserProfilePanel.transform.Find("StatsLayoutGroup").transform.Find("TapsLayoutGroup").transform.Find("TapsText").GetComponent<Text>();
    private Button UserProfilePanel_Quit => UserProfilePanel.transform.Find("NameAndQuitLayoutGroup").transform.Find("QuitButton").GetComponent<Button>();
    private Button UserProfilePanel_AddFriend => UserProfilePanel.transform.Find("ButtonsLayoutGroup").transform.Find("AddButton").GetComponent<Button>();
    private Button UserProfilePanel_Unfriend => UserProfilePanel.transform.Find("ButtonsLayoutGroup").transform.Find("UnfriendButton").GetComponent<Button>();
    private Button UserProfilePanel_Fight => UserProfilePanel.transform.Find("ButtonsLayoutGroup").transform.Find("FightButton").GetComponent<Button>();

    private List<PlayerInfo> top5Uusers = new List<PlayerInfo>();
    private List<GameObject> top5UsersGO = new List<GameObject>();

    private List<GameObject> SearchedUsersGO = new List<GameObject>();
    private List<PlayerInfo> SearchedUsers = new List<PlayerInfo>();
    private bool IsToUpdateSearchedUsersList = false;

    private bool IsToUpdateTop5List = false;
    private int rank = 999;

    void Start()
    {
        var highestScore = PlayerPrefs.GetFloat(PlayerPrefsKeys.SINGLE_HIGHEST_SCORE_KEY);
        HghestScoreText.GetComponent<Text>().text = highestScore == 0.0f ? "-" : highestScore.ToString("f0");

        var maxTaps = PlayerPrefs.GetFloat(PlayerPrefsKeys.SINGLE_MAX_TAPS_KEY);
        MaxTapsText.GetComponent<Text>().text = (maxTaps == 0.0f ? "-" : maxTaps.ToString("f0")) + " taps in row";

        if (ConnectionHelper.HasInternet)
        {
            ServerManager.Instance.Client.Send(URI.GetTopFive, null, OnGetTopFive);
        }
        else
        {
            NoConnectionImage.SetActive(true);
        }
    }

    void Update()
    {
        if (IsToUpdateTop5List && top5Uusers.Any())
        {
            IsToUpdateTop5List = false;
            RenderTop5UsersList();
        }

        if (IsToUpdateSearchedUsersList)
        {
            IsToUpdateSearchedUsersList = false;
            RenderSearchedUsersList();
        }
    }

    #region Top best 5

    private void OnGetTopFive(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            top5Uusers.Clear();
            var dto = p.DeserializeContent<TopFiveDto>();
            top5Uusers.AddRange(dto.TopFive);
            rank = dto.PlayerRank;
            if (dto.FriendsIds != null)
            {
                foreach (var friendId in dto.FriendsIds)
                    MyPlayer.Instance.AddFriend(friendId);
            }

            IsToUpdateTop5List = true;
        }
    }

    private void RenderTop5UsersList()
    {
        CleanUpUsers();
        CleanUpTop5();

        bool selfOnTop = false;
        int position = 1;
        foreach (PlayerInfo f in top5Uusers.OrderByDescending(u => u.SinglePlayerHighestScore).ToArray())
        {
            GameObject go = Instantiate(UserGO, UserContainer.transform) as GameObject;
            go.GetComponentInChildren<Text>().text = position + ". " + f.PlayerName;
            ++position;
            Button b = go.GetComponent<Button>();

            if (f.ID == MyPlayer.Instance.Info.ID)
            {
                selfOnTop = true;
                b.interactable = false;
            }
            else
            {
                string playerState = f.CurrentState == PlayerState.Offline ? "OfflineImage" : "OnlineImage";
                go.transform.Find(playerState).gameObject.SetActive(true);

                b.onClick.AddListener(() =>
                {
                    var isFriend = MyPlayer.Instance.IsFriendOf(f.ID);
                    FillProfilePanel(f, isFriend);
                });
            }

            top5UsersGO.Add(go);
        }

        GameObject reticencias = Instantiate(UserGO, UserContainer.transform) as GameObject;
        reticencias.GetComponentInChildren<Text>().text = "...";
        reticencias.GetComponent<Button>().interactable = false;
        top5UsersGO.Add(reticencias);

        if (!selfOnTop)
        {
            GameObject self = Instantiate(UserGO, UserContainer.transform) as GameObject;
            self.GetComponentInChildren<Text>().text = rank + ". " + MyPlayer.Instance.Info.PlayerName;
            self.GetComponent<Button>().interactable = false;
            top5UsersGO.Add(self);
        }
    }

    private void CleanUpTop5()
    {
        foreach (GameObject go in top5UsersGO)
        {
            Destroy(go);
        }

        top5UsersGO.Clear();
    }

    private void FillProfilePanel(PlayerInfo f, bool isFriend)
    {
        UserProfilePanel_Name.text = f.PlayerName;
        UserProfilePanel_HighestScore.text = f.SinglePlayerHighestScore.ToString();
        UserProfilePanel_Taps.text = f.MaxHitsInRowSinglPlayer.ToString();
        BuildProfilePanelButtons(f, isFriend);

        UserProfilePanel_Quit.onClick.AddListener(() =>
        {
            UserProfilePanel.SetActive(false);
        });

        UserProfilePanel.transform.position = Input.mousePosition;
        UserProfilePanel.SetActive(true);
    }

    //isFriend -> -1 = Friends not loaded
    //isFriend -> 0 = false
    //isFriend -> 1 = true
    private void BuildProfilePanelButtons(PlayerInfo f, bool isFriend)
    {
        UserProfilePanel_Unfriend.onClick.RemoveAllListeners();
        UserProfilePanel_Fight.onClick.RemoveAllListeners();
        UserProfilePanel_AddFriend.onClick.RemoveAllListeners();

        UserProfilePanel_Fight.gameObject.SetActive(false);
        /* TODO
        UserProfilePanel_Fight.gameObject.SetActive(f.CurrentState == PlayerState.Online);
        UserProfilePanel_Fight.onClick.AddListener(() =>
        {
            ChallengeRequest(f.ID);
        });
        */

        if (isFriend)
        {
            UserProfilePanel_AddFriend.gameObject.SetActive(false);
            UserProfilePanel_Unfriend.gameObject.SetActive(true);
            UserProfilePanel_Unfriend.onClick.AddListener(() =>
            {
                ServerManager.Instance.Client.Send(URI.Unfriend, f.ClientID, OnUnfriend);
                UserProfilePanel.SetActive(false);
            });
        }
        else
        {
            UserProfilePanel_AddFriend.gameObject.SetActive(true);
            UserProfilePanel_Unfriend.gameObject.SetActive(false);
            UserProfilePanel_AddFriend.onClick.AddListener(() =>
            {
                ServerManager.Instance.Client.Send(URI.AddFriend, f.ClientID, OnAddFriend);
                UserProfilePanel.SetActive(false);
            });
        }
    }

    private void OnUnfriend(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            PlayerInfo removed = p.DeserializeContent<PlayerInfo>();
            MyPlayer.Instance.RemoveFriend(removed.ID);
            IsToUpdateTop5List = true;

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
            MyPlayer.Instance.AddFriend(added.ID);

            IsToUpdateTop5List = true;
            UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("Uuuuuh so needy.", 2f, MessagePanel)));
        }
        else
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("Couldn't add user.", 2f, MessagePanel)));
        }

    }

    #endregion

    #region Search User

    private void RenderSearchedUsersList()
    {
        IsToUpdateSearchedUsersList = false;

        CleanUpTop5();
        CleanUpUsers();

        //NoResults.SetActive(false);

        if (!SearchedUsers.Any())
        {
            //NoResults.SetActive(true);
        }
        else
        {
            foreach (PlayerInfo pi in SearchedUsers.Where(u => u.ID != MyPlayer.Instance.Info.ID).ToArray())
            {
                GameObject go = Instantiate(UserGO, UserContainer.transform) as GameObject;
                go.GetComponentInChildren<Text>().text = pi.PlayerName;
                SearchedUsersGO.Add(go);
                Button b = go.GetComponent<Button>();

                string playerState = pi.CurrentState == PlayerState.Offline ? "OfflineImage" : "OnlineImage";
                go.transform.Find(playerState).gameObject.SetActive(true);

                b.onClick.AddListener(() =>
                {
                    var isFriend = MyPlayer.Instance.IsFriendOf(pi.ID);
                    FillProfilePanel(pi, isFriend);
                });
            }
        }
    }


    private void CleanUpUsers()
    {
        foreach (GameObject go in SearchedUsersGO)
        {
            Destroy(go);
        }

        SearchedUsersGO.Clear();
    }

    public void SearchPlayers(string value)
    {
        if (!ConnectionHelper.HasInternet)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("You need to connect bro", 3f, MessagePanel))); // TODO create MessagePanel on Single Player Menu and attach to this script
            return;
        }

        if (value.Length >= 3)
        {
            ServerManager.Instance.Client.Send(URI.SearchPlayer, value, OnSearchPlayer);
        }
        else
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("Give me some more letters pease", 3f, MessagePanel))); // TODO create MessagePanel on Single Player Menu and attach to this script
        }
    }


    private void OnSearchPlayer(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            SearchedUsers.Clear();
            ArrayWrapper mi = p.DeserializeContent<ArrayWrapper>();
            SearchedUsers.AddRange(mi.GetArray<PlayerInfo>().OrderBy(pi => pi.PlayerName));
            IsToUpdateSearchedUsersList = true;
        }
        else
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("Sorry, some Internet devil blew up the request to the server", 3f, MessagePanel))); // TODO create MessagePanel on Single Player Menu and attach to this script
        }
    }

    #endregion


    public void OnStartClick()
    {
        SceneManager.LoadScene("Single Player");
    }

    public void OnBackClick()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
