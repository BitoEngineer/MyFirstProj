using Assets.Scripts.Preloader;
using Assets.Scripts.Utils;
using Assets.Server.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUIController : MonoBehaviour
{

    public GameObject NameText;
    public GameObject PointsText;
    public GameObject OnTouchPointsReceivedText;
    private int points = -1;

    public static PlayerStatsUIController Instance;

    void Start ()
    {
        Instance = this;
        NameText.GetComponent<Text>().text = MyPlayer.Instance.Info.PlayerName;
        points = 0;
    }

    public void UpdatePoints(int pointsUpdated)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            var pointsDifference = pointsUpdated - this.points;
            this.points = pointsUpdated;

            PointsText.GetComponent<Text>().text = pointsUpdated.ToString();

            string pointsText = "";
            var pointsReceivedText = OnTouchPointsReceivedText.GetComponent<Text>();
            if (pointsDifference > 0)
            {
                pointsText = "+" + pointsDifference;
                pointsReceivedText.color = Color.green;
            }
            else
            {
                pointsText = "" + pointsDifference;
                pointsReceivedText.color = Color.red;
            }

            StartCoroutine(UIUtils.ShowMessageInText(pointsText, 1f, pointsReceivedText));
        });
    }
}
