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
    public static CountDown Instance { set; get; }

    // Use this for initialization
    void Start () {
        Instance = this;
        startTime = Time.time;
        countdown.enabled = true;
    }
	
	// Update is called once per frame
	void Update () {
        float t = Time.time - startTime;
        if (Counting)
        {
            int seconds = 4- int.Parse((t % 60).ToString("f0"));
            if(seconds!=4)
                countdown.text = seconds.ToString();
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
                ServerManager.Instance.Client.Send(URI.ChallengeReply, cr);
            }
        }

        if (t > 3f)
        {
            if (Counting)
                TimerCounter.Instance.ResumeTimer();
            Counting = false;
            
        }
    }

    public void StartCountDown(bool mp)
    {
        this.mp = mp;
        TimerCounter.Instance.StopTimer();
        startTime = Time.time;
        countdown.text = "";
        countdown.enabled = true;

        Counting = true;
    }
}
