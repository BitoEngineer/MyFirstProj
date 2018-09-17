using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerCounter : MonoBehaviour {

    public Text timerText;
    public float startTime;
    private TouchManager touchM;
    private bool active = true;
    private float t;

	// Use this for initialization
	void Start () {
        startTime = Time.realtimeSinceStartup;
        touchM = GameObject.Find("Touch").GetComponent<TouchManager>();
    }
	
	// Update is called once per frame
	void Update () {

        if (active)
        {
            t = Time.realtimeSinceStartup - (startTime ) ;

            string minutes = ((int)t / 60).ToString();
            string seconds = (t % 60).ToString("f2");
            string sec= seconds.Split('.')[0];
            sec= sec.Length > 1 ? sec : "0" + sec;
            string ms=seconds.Split('.')[1];
            ms = ms.Length > 1 ? ms : "0" + ms;
            timerText.text = minutes + ":" + sec + "."+ms;
        }
        
	} 

    public void StopTimer()
    {
        active = false;

    }

    public void ResumeTimer(float afterSec = 0)
    {
        active = true;
    }
}
