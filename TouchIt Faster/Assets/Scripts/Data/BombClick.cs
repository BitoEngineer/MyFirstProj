using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombClick : MonoBehaviour {

    private TouchManager touchM;

    //Use this for initialization
    void Start()
    {
        touchM = GameObject.Find("Background").GetComponent<TouchManager>();
    }

    private void OnMouseDown()
    {
        touchM.BombTouched(gameObject);
    }
}
