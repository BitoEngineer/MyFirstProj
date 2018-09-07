using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUIController : MonoBehaviour
{

    public Text PointsText;
    private int points = -1;

    public static PlayerStatsUIController Instance;

    void Start ()
    {
        Instance = this;
        points = 0;
    }
	
	
	void Update () {
	    if (points != -1)
	    {
	        PointsText.text = points.ToString();
	        points = -1;
	    }
	}

    public void UpdatePoints(int points)
    {
        this.points = points;
    }
}
