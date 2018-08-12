using Assets.Server.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assets.Server.Protocol;
using UnityEngine.UI;
using System;
using MyFirstServ.Models.TouchItFaster;

public class MultiplayerManager : MonoBehaviour {

    public GameObject FriendGO;
    public GameObject FriendsContainer;
    public GameObject SearchContainer;

    private List<PlayerInfo> Friends = new List<PlayerInfo>();
    private List<GameObject> FriendsGO = new List<GameObject>();
    private List<GameObject> SearchedGO = new List<GameObject>();

    private bool FriendsUpdated = false, SearchedPlayers = false;

	// Use this for initialization
	void Start () {
        UpdateFriends();
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
                b.onClick.AddListener(()=>
                {
                    ChallengeRequest cr = new ChallengeRequest()
                    {
                        RequesterID = PlayerContainer.Instance.Info.ID,
                        RequestedID = f.ID
                    };
                    ServerManager.Instance.Client.Send(URI.ChallengeRequest, cr, ChallengeRequestReply);
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

    private void ChallengeRequestReply(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            var reply = p.DeserializeContent<ChallengeReply>();

            if (reply.Reply == ChallengeReplyType.ChallengeAccepted)
            {
                ChangeScene("MultiplayerInGame");
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
}
