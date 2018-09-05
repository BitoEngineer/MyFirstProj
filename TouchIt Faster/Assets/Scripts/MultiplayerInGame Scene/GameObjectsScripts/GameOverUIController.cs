using System.Collections;
using System.Collections.Generic;
using Assets.Server.Models;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUIController : MonoBehaviour
{
    public static GameOverUIController Instance;

    private Text PointsText;


	void Start ()
	{
	    Instance = this;
	}
	

	void Update () {
		
	}

    public void GameOverUpdate(GameOver gameOver)
    {

    }
}
