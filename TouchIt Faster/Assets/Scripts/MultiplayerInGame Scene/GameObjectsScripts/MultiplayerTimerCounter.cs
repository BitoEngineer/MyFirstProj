using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerTimerCounter : MonoBehaviour
{

    public Text TimerText;

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
            float t = StartTime - Time.deltaTime;

            string minutes = ((int)t / 60).ToString();
            string seconds = (t % 60).ToString("f2");
            string sec = seconds.Split('.')[0];
            sec = sec.Length > 1 ? sec : "0" + sec;
            string ms = seconds.Split('.')[1];
            ms = ms.Length > 1 ? ms : "0" + ms;

            if (t < 10f)
            {
                TimerText.color = new Color32(170, 36, 41, 255); /*Red*/
            }
            else
            {
                TimerText.color = Color.white;
            }

            TimerText.text = minutes + ":" + sec + "." + ms;
        }

    }

    public void UpdateTimer(float secs)
    {
        StartTime = secs;
    }

    public void StopTimer()
    {
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
}
