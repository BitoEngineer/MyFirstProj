using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SinglePlayerMenuManager : MonoBehaviour
{
    void Start()
    {
        //TODO request top 10        
    }

    public void OnStartClick()
    {
        SceneManager.LoadScene("Single Player");
    }

    public void OnBackClick()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
