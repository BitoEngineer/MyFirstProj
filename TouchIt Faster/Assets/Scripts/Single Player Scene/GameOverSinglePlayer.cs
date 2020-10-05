using Assets.Scripts.Single_Player_Scene.Models;
using Assets.Server.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverSinglePlayer : MonoBehaviour {

    public Text GameOverNameText;
    public Text GameOverPointsText;
    public Text GameOverTapsInRowText;
    public Text GameOverHighestPointsText;

    private Game Game;

    void Start ()
    {
        Game = GameObject.Find("Touch").GetComponent<TouchManager>().GetGame();

        GameObject.Find("SpawnerCanvas").SetActive(false);

        GameOverPointsText.text = Game.Points.ToString("f0");
        GameOverTapsInRowText.text = Game.TapsInRow.ToString("f0") + " IN ROW";
        GameOverHighestPointsText.text = Game.HighestScore.ToString("f0");
    }

    public void OnBackClick()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void OnTryAgainClick()
    {
        SceneManager.LoadScene("Single player");
    }
}
