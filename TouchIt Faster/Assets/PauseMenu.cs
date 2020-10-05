using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    
    public GameObject timerCounter;
    public GameObject pauseMenu;

    private TouchManager touchManager;

    public void Start()
    {
        touchManager = GameObject.Find("Touch").GetComponent<TouchManager>();
    }

    public void Resume()
    {
        GameObject.Find("SpawnerCanvas").SetActive(true);
        touchManager.Resume();
        pauseMenu.SetActive(false);
    }

    public void Pause()
    {
        touchManager = GameObject.Find("Touch").GetComponent<TouchManager>();
        touchManager.Pause();

        GameObject.Find("SpawnerCanvas").SetActive(false);
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
