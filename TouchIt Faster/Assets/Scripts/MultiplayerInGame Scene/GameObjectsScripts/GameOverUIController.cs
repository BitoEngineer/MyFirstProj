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

public class GameOverUIController : MonoBehaviour
{
    public static GameOverUIController Instance;

    public GameObject GameOverPanel;
    public GameObject MessagePanel;
    public Text PlayerStatsText;
    public Text OpponentStatsText;
    public Button MainMenuBtn;
    public Button AddOpponentBtn;
    public Button RevengeBtn;


	void Start ()
	{
        AddOpponentBtn.onClick.AddListener(AddOpponentListener);
        RevengeBtn.onClick.AddListener(RevengeListener);
        MainMenuBtn.onClick.AddListener(MainMenuListener);
	    Instance = this;
	}

    public void GameOverUpdate(GameOver gameOver)
    {

        MultiplayerTimerCounter.Instance.StopTimer();
        PlayerStatsText.text = FillStatsString(gameOver, PlayerStatsText.text, false);
        PlayerStatsText.text = FillStatsString(gameOver, OpponentStatsText.text, true);

        GameOverPanel.SetActive(true);
    }

    private string FillStatsString(GameOver gameOver, string pst, bool opponent)
    {
        if (!opponent)
        {
            pst.Replace("PTS", gameOver.Points.ToString());
            pst.Replace("TL", gameOver.TimeLeft.ToString());
            pst.Replace("TLEP", gameOver.TimePoints.ToString());
            pst.Replace("HR", gameOver.TapsInARow.ToString());
        }
        else
        {
            pst.Replace("PTS", gameOver.OpponentPoints.ToString());
            pst.Replace("TL", gameOver.OpponentTimeLeft.ToString());
            pst.Replace("TLEP", gameOver.OpponentTimePoints.ToString());
            pst.Replace("HR", gameOver.OpponentTapsInARow.ToString());
        }

        return pst;
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
                UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("Hmmm let's see if he is capable...", 2f, MessagePanel)));
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

    public void MainMenuListener()
    {
        SceneManager.LoadScene("Main Menu");
    }

}
