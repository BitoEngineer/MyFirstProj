using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Multiplayer_Scene;
using Assets.Scripts.Preloader;
using Assets.Scripts.Utils;
using Assets.Server.Models;
using Assets.Server.Protocol;
using MyFirstServ.Models.TouchItFaster;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GameOverDTO = Assets.Server.Models.GameOver;

public class GameOverUIController : MonoBehaviour
{
    public static GameOverUIController Instance;

    public GameObject SpawnerCanvas;
    public GameObject DisableAllPanel;
    public GameObject GameOverPanel;
    public GameObject MessagePanel;
    public GameObject ChallengeRequestPanel;
    public Button MainMenuBtn;
    public Button RevengeBtn;


    void Start()
    {
        SpawnerCanvas.SetActive(false);
        DisableAllPanel.SetActive(true);
        RevengeBtn.onClick.AddListener(RevengeListener);
        MainMenuBtn.onClick.AddListener(MainMenuListener);
        Instance = this;

        ServerManager.Instance.Client.AddCallback(URI.ChallengeRequest, OnChallengeRequest);
    }

    public void GameOverUpdate(GameOverDTO gameOver)
    {
        var winnerpanel = transform.Find("WinnerPanel");
        var loserpanel = transform.Find("LoserPanel");

        bool playerLost = gameOver.OpponentPoints > gameOver.Points;

        FillStatsPanel(playerLost ? winnerpanel : loserpanel, GameContainer.OpponentName, gameOver.OpponentPoints, gameOver.OpponentTimePoints, gameOver.OpponentTapsInARow);
        FillStatsPanel(playerLost ? loserpanel : winnerpanel, PlayerContainer.Instance.Info.PlayerName, gameOver.Points, gameOver.TimePoints, gameOver.TapsInARow);
    }

    private void FillStatsPanel(Transform panel, string playername, int points, int extraPoints, int hitInRow)
    {
        panel.Find("StatsAndNamePanel/NameText").GetComponent<Text>().text = playername.ToUpperInvariant();
        panel.Find("StatsAndNamePanel/StatsPanel/PointsPanel/PointsText").GetComponent<Text>().text = "" + points;
        panel.Find("StatsAndNamePanel/StatsPanel/PointsPanel/ExtraPointsText").GetComponent<Text>().text = "(+" + extraPoints + ")";
        panel.Find("StatsAndNamePanel/StatsPanel/TapsInRowText").GetComponent<Text>().text = "" + hitInRow + " IN ROW";
    }

    public void AddOpponentListener()
    {
        ServerManager.Instance.Client.Send(URI.AddFriend, GameContainer.OpponentID, AddFriendReply);
    }

    private void AddFriendReply(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            PlayerInfo added = p.DeserializeContent<PlayerInfo>();
            UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("Uuuuuh so needy.", 2f, MessagePanel)));
        }
        else
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("Couldn't add user.", 2f, MessagePanel)));
        }
    }

    public void RevengeListener()
    {
        ChallengeRequest cr = new ChallengeRequest()
        {
            RequesterID = PlayerContainer.Instance.Info.ID,
            RequestedID = GameContainer.OpponentID
        };
        ServerManager.Instance.Client.Send(URI.ChallengeRequest, cr, RevengeReply);
    }

    public void RevengeReply(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            var reply = p.DeserializeContent<ChallengeReply>();

            if (reply.Reply == ChallengeReplyType.Waiting)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("Hmmm let's see if he has the balls..", 2f, MessagePanel)));
            }
            else if (reply.Reply == ChallengeReplyType.ChallengeAccepted)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("Be faster this time!", 2f, MessagePanel)));
                GameContainer.CurrentGameId = reply.ChallengeID;
                ServerManager.Instance.NextScene = "MultiplayerInGame";
            }
            else if (reply.Reply == ChallengeReplyType.ChallengeRefused || reply.Reply == ChallengeReplyType.ChallengeRefused)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("Ahah, that boy is affraid", 2f, MessagePanel)));
            }
        }
        else if (p.ReplyStatus == ReplyStatus.Forbidden)
        {
            /*TODO*/
        }
    }

    public void MainMenuListener()
    {
        SceneManager.LoadScene("Multiplayer");
    }

    private void OnChallengeRequest(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            var cr = p.DeserializeContent<ChallengeRequest>();

            ChallengeRequestPanel.transform.Find("ButtonsPanel/YesButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log("Multiplayer Menu: Accepting friend's challenge");
                ChallengeReply creply = new ChallengeReply()
                {
                    ChallengeID = cr.ID,
                    Reply = ChallengeReplyType.ChallengeAccepted
                };
                ServerManager.Instance.Client.Send(URI.ChallengeReply, creply);

                UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("Be faster this time!", 2f, MessagePanel)));
                GameContainer.CurrentGameId = creply.ChallengeID;
                ServerManager.Instance.NextScene = "MultiplayerInGame";
            });

            ChallengeRequestPanel.transform.Find("ButtonsPanel/NoButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log("Multiplayer Menu: Declining friend's challenge");
                ChallengeReply creply = new ChallengeReply()
                {
                    ChallengeID = cr.ID,
                    Reply = ChallengeReplyType.ChallengeRefused
                };
                ServerManager.Instance.Client.Send(URI.ChallengeReply, creply);
            });

            UnityMainThreadDispatcher.Instance().Enqueue(() => ChallengeRequestPanel.SetActive(true));
        }
    }
}
