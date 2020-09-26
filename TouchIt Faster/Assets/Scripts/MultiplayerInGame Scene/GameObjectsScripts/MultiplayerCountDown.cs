using Assets.Scripts.Multiplayer_Scene;
using Assets.Scripts.Preloader;
using Assets.Server.Protocol;
using MyFirstServ.Models.TouchItFaster;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerCountDown : MonoBehaviour
{

    public Text countdown;
    public float startTime;
    private bool mp = false;
    public bool Counting = true;
    public GameObject TimerCounterGO;

    // Use this for initialization
    void Start()
    {
        startTime = Time.time;
        countdown.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        float t = Time.time - startTime;
        if (Counting)
        {
            int seconds = 4 - (int)(t % 60);
            if (seconds != 4)
            {
                countdown.text = seconds.ToString();
            }
        }
        else
        {
            countdown.enabled = false;
            if (mp)
            {
                mp = false;
                ChallengeReply cr = new ChallengeReply()
                {
                    ChallengeID = GameContainer.CurrentGameId,
                    Reply = ChallengeReplyType.Start
                };
                ServerManager.Instance.Client.Send(URI.ChallengeReply, cr, OnStartMatch);
            }
        }

        if (t > 3f)
        {
            Counting = false;
        }
    }

    public void StartCountDown(bool mp)
    {
        this.mp = mp;
        TimerCounterGO.GetComponent<MultiplayerTimerCounter>().StopTimer();
        startTime = Time.time;
        countdown.text = "";
        countdown.enabled = true;

        Counting = true;
    }

    public void OnStartMatch(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            ChallengeReply reply = p.DeserializeContent<ChallengeReply>();
            
            GameContainer.SetOpponent(reply.OpponentID, reply.OpponentName);

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                TimerCounterGO.GetComponent<MultiplayerTimerCounter>().ResumeTimer();
            });
        }
    }
}
