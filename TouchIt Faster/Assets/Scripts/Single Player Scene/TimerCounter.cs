using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerCounter : MonoBehaviour {

    public Text timerText;
    public float startTime;
    private TouchManager touchM;
    private bool active = true;

	// Use this for initialization
	void Start () {
        startTime = Time.time;
        touchM = GameObject.Find("Touch").GetComponent<TouchManager>();
    }
	
	// Update is called once per frame
	void Update () {
        if (touchM.OnPause)
        {
             StopTimer();
        }
        else
        {
            ResumeTimer();
        }

        if (active)
        {
            float t = Time.time - startTime;

            string minutes = ((int)t / 60).ToString();
            string seconds = (t % 60).ToString("f2");

            timerText.text = minutes + ":" + seconds;
        }
        
	} 

    public void StopTimer()
    {
        active = false;
    }

    public void ResumeTimer()
    {
        active = true;
    }
}
