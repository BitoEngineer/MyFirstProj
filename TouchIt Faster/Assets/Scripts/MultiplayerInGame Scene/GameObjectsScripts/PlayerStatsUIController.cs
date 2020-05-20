using Assets.Server.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUIController : MonoBehaviour
{

    public GameObject NameText;
    public GameObject PointsText;
    private int points = -1;

    public static PlayerStatsUIController Instance;

    void Start ()
    {
        Instance = this;
        NameText.GetComponent<Text>().text = PlayerContainer.Instance.Info.PlayerName;
        points = 0;
    }
	
	
	void Update () {
	    if (points != -1)
	    {
	        PointsText.GetComponent<Text>().text = points.ToString();
	        points = -1;
	    }
	}

    public void UpdatePoints(int points)
    {
        this.points = points;
    }
}
