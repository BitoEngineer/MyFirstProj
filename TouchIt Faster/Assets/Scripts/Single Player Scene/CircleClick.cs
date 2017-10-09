using Assets.Server.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CircleClick : MonoBehaviour {

    private TouchManager touchM;

    //Use this for initialization
    void Start () {
          touchM = GameObject.Find("Touch").GetComponent<TouchManager>();
    }

    private void OnMouseDown()
    {
        //ServerManager.Instance.Send(0, new MousePosition(){ x = Input.mousePosition.x, y = Input.mousePosition.y });
        if (!touchM.OnPause)
        {
            touchM.PointsUpdate(gameObject);
            Destroy(gameObject); 
        }
              
    }


}
