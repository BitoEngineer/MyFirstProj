using Assets.Scripts.Multiplayer_Scene;
using Assets.Server.Protocol;
using MyFirstServ.Models.TouchItFaster;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountDown : MonoBehaviour {

    public Text countdown;
    public float startTime;
    private bool mp = false;
    public bool Counting = true;
    public GameObject TimerCounterGO;

    // Use this for initialization
    void Start () {
        startTime = Time.time;
        countdown.enabled = true;
    }
	
	// Update is called once per frame
	void Update ()
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
            if (Counting)
                TimerCounterGO.GetComponent<TimerCounter>().ResumeTimer();
            Counting = false;
            
        }
    }

    public void StartCountDown(bool mp)
    {
        this.mp = mp;
        TimerCounterGO.GetComponent<TimerCounter>().StopTimer();
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
            GameContainer.OpponentID = reply.OpponentID;
        }
    }
}
