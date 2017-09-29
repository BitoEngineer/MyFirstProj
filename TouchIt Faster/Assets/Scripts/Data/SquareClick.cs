using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SquareClick : MonoBehaviour {

    private TouchManager touchM;

    //Use this for initialization
    void Start()
    {
        touchM = GameObject.Find("Background").GetComponent<TouchManager>();
    }

    private void OnMouseDown()
    {
        touchM.SquareTouched(gameObject);
        Destroy(gameObject);

    }
}
