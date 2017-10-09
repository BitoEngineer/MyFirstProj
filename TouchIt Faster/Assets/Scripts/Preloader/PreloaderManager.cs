using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PreloaderManager : MonoBehaviour {

    bool t = true;

	// Use this for initialization
	void Start () {
    }
	
	// Update is called once per frame
	void Update () {
		if(t)
        {
            t = false;
            SceneManager.LoadScene("Main Menu");
        }
	}
}
