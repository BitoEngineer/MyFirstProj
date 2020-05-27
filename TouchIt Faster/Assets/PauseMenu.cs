using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    private TouchManager touchManager;
    
    public GameObject timerCounter;
    public GameObject pauseMenu;

    // Start is called before the first frame update
    public void Start()
    {
        touchManager = GameObject.Find("Touch").GetComponent<TouchManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Resume()
    {
        timerCounter.GetComponent<TimerCounter>().ResumeTimer();
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        touchManager.OnPause = false;
    }

    public void Pause()
    {
        touchManager = GameObject.Find("Touch").GetComponent<TouchManager>();
        touchManager.OnPause = true;
        timerCounter.GetComponent<TimerCounter>().StopTimer();
        Time.timeScale = 0f;
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
