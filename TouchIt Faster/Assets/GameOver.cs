using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public GameObject gameOverPanel;
    private TouchManager touchManager;

    // Start is called before the first frame update
    void Start()
    {
        touchManager = GameObject.Find("Touch").GetComponent<TouchManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Continue(GameObject countDown)
    {    
        if (!touchManager.AdSeen)
        {
            //AdManager.Instance.showInterstitialAd();
            touchManager.AdSeen = true;
            gameOverPanel.SetActive(false);

            touchManager.CleanUpCircles();
            touchManager.Life1.enabled = false;
            touchManager.Life2.enabled = false;
            touchManager.Life3.enabled = false;
            touchManager.OnPause = false;
            touchManager.Lifes = 3;
            //timerCounter.GetComponent<TimerCounter>().ResumeTimer(); ;
            countDown.GetComponent<CountDown>().StartCountDown(false);
        }
        else
        {
            //Generate text saying that AD already was used
        }

    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void Restart()
    {
        SceneManager.LoadScene("Single player");
    }
}
