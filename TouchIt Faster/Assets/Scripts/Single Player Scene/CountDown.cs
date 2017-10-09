using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountDown : MonoBehaviour {

    public Text countdown;
    public float startTime;
    public bool counting = true;
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
        if (counting)
        {
            int seconds = 4- int.Parse((t % 60).ToString("f0"));
            if(seconds!=4)
                countdown.text = seconds.ToString();
        }
        else
        {
            countdown.enabled = false;
        }

        if (t > 3f)
        {
            if (counting)
                TimerCounter.Instance.ResumeTimer();
            counting = false;
        }
    }

    public void StartCountDown()
    {
        TimerCounter.Instance.StopTimer();
        startTime = Time.time;
        countdown.text = "";
        countdown.enabled = true;
        
        counting = true;
    }
}
