using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountDown : MonoBehaviour {

    public Text countdown;
    public float startTime;
    public bool counting = true;

    // Use this for initialization
    void Start () {
        startTime = Time.time;
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
            counting = false;
        }
    }
}
