using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverSinglePlayer : MonoBehaviour {

    private float Points;
    public Text Score;

    // Use this for initialization
    void Start () {
        Points = PlayerPrefs.GetFloat("points", 0);
        Score.text += Points.ToString("f0");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
