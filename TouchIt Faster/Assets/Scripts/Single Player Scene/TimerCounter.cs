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
    public static TimerCounter Instance { get; set; }
	// Use this for initialization
	void Start () {
        Instance = this;
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

            timerText.text = minutes + ":" + seconds;
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
