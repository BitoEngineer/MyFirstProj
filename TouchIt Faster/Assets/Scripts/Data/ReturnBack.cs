using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnBack : MonoBehaviour {

    public GameObject ExitPopup;
    private TouchManager touchM;
    public GameObject TimerCounterGO;

    private void Start()
    {
        touchM = GameObject.Find("Touch").GetComponent<TouchManager>();

    }


    public void OnMouseDown()
    {
        touchM.OnPause = true;
        //TimerCounter.Instance.StopTimer(); TODO
        ExitPopup.SetActive(true);
    }

    public void Leave()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void Resume()
    {
        ExitPopup.SetActive(false);
        touchM.OnPause = false;
        TimerCounterGO.GetComponent<TimerCounter>().ResumeTimer();

    }
}
