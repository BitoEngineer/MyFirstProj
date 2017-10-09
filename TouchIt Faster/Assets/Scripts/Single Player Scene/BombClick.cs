using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombClick : MonoBehaviour {

    private TouchManager touchM;

    //Use this for initialization
    void Start()
    {
        touchM = GameObject.Find("Touch").GetComponent<TouchManager>();
    }

    public void OnPointerEnter()
    {
        if(!touchM.OnPause) 
            touchM.BombTouched(gameObject);
    }
}
