using Assets.Server.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CircleClick : MonoBehaviour {

    private TouchManager touchM;

    //Use this for initialization
    void Start () {
          touchM = GameObject.Find("Touch").GetComponent<TouchManager>();
    }

    public void OnPointerEnter()
    {
        if (!touchM.OnPause)
        {
            touchM.PointsUpdate(gameObject);
            Destroy(gameObject);
        }
    }

}
