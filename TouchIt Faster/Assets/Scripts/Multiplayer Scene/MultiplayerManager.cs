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
                    ServerManager.Instance.Send(TouchItFasterContentType.ChallengeRequest, cr, ChallengeRequestReply);
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

    private void ChallengeRequestReply(JsonPacket p, ServerManager.ReplyResult result)
    {
        if (result == ServerManager.ReplyResult.Success)
        {
            if (p.ContentType == (int)TouchItFasterContentType.ChallengeReply)
            {
                var reply = p.DeserializeContent<ChallengeReply>();

                if (reply.Reply == ChallengeReplyType.Waiting)
                {
                    ServerManager.Instance.PacketReceivedEvent += GamePacketReceived;
                }
            }
            else
            {
                //invalid content type
            }

        }
    }

    private void GamePacketReceived(JsonPacket p, ServerManager.ReplyResult result)
    {
        if (result == ServerManager.ReplyResult.Success)
        {
            if (p.ContentType == (int)TouchItFasterContentType.ChallengeReply)
            {
                var reply = p.DeserializeContent<ChallengeReply>();

                if (reply.Reply == ChallengeReplyType.ChallengeAccepted)
                {
                    ChangeScene("MultiplayerInGame");
                }
            }
            else
            {
                //invalid content type
            }

        }
    }

    public void UpdateFriends()
    {
        Request r = new Request()
        {
            ReqType = RequestType.Friends
        };
        ServerManager.Instance.Send(TouchItFasterContentType.Request, r, RequestFriendsNamesReply);
    }

    private void RequestFriendsNamesReply(JsonPacket p, ServerManager.ReplyResult result)
    {
        if (result ==ServerManager.ReplyResult.Success){
            if (p.ContentType == (int)TouchItFasterContentType.Friends)
            {
                Friends.Clear();
                try
                {
                    ArrayWrapper mi = p.DeserializeContent<ArrayWrapper>();
                    Friends.AddRange(mi.GetArray<PlayerInfo>());
                    FriendsUpdated = true;
                }
                catch(Exception e)
                {
                    Debug.Log("Fódeu gerau");
                }               

            }
            else
            {
                //invalid content type
            }

        }

    }

    public void SearchPlayers(string value)
    {
        //ServerManager.Instance.Send((byte)TouchItFasterContentType.SearchPlayer, new PlayerSearch() { Key = value }, PlayerSearchResult);
    }


    private void PlayerSearchResult(JsonPacket p, ServerManager.ReplyResult result)
    {
        if (result == ServerManager.ReplyResult.Success)
        {
            //ps = p.DeserializeContent<PlayerSearch>();
            SearchedPlayers = true;
        }
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
