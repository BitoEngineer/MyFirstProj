using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Preloader;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerTimerCounter : MonoBehaviour
{
    public Text TimerText;
    public Text ExtraTimeText;

    private float StartTime = 60f;
    private bool Active = false;

    public static MultiplayerTimerCounter Instance { get; set; }

    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Active)
        {
            StartTime -= Time.deltaTime;

            string minutes = ((int)StartTime / 60).ToString();
            string seconds = (StartTime % 60).ToString("f1");
            string sec = seconds.Split('.')[0];
            sec = sec.Length > 1 ? sec : "0" + sec;
            string ms = seconds.Split(',')[1];
            ms = ms.Length > 1 ? ms : "0" + ms;

            if (StartTime < 10f)
            {
                TimerText.color = new Color32(170, 36, 41, 255); /*Red*/

                if(StartTime <= 0f)
                {
                    StopTimer();
                }
            }

            TimerText.text = minutes + ":" + sec;// + "." + ms;
        }

    }

    public void UpdateTimer(float secs)
    {
        StartTime = secs;
    }

    public void StopTimer()
    {
        var secs = double.Parse(TimerText.text.Split(':')[1]);
        TimerText.text = secs < 0 ? "0:00" : TimerText.text;
        Active = false;
    }

    public void ResumeTimer(float afterSec = 0)
    {
        Active = true;
    }

    public bool IsActive()
    {
        return Active;
    }

    public void ExtraTime(float extratime)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            string msg;
            if (extratime > 0)
            {
                msg = "+" + extratime.ToString("0.##");
                ExtraTimeText.color = Color.green;
            }
            else
            {
                msg = extratime.ToString("0.##");
                ExtraTimeText.color = Color.red;
            }

            StartCoroutine(UIUtils.ShowMessageInText(msg, 1f, ExtraTimeText));
        });
    }
}
