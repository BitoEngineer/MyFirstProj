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
          touchM = GameObject.Find("Background").GetComponent<TouchManager>();
    }

    private void OnMouseDown()
    {
            touchM.PointsUpdate(gameObject);
            Destroy(gameObject);   
    }


}
