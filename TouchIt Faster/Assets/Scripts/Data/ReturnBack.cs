using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnBack : MonoBehaviour {

    public GameObject PauseMenu;

    private void Start()
    {
        PauseMenu.GetComponent<PauseMenu>();
    }


    public void OnMouseDown()
    {
        PauseMenu.SetActive(true);
        PauseMenu.GetComponent<PauseMenu>().Pause();        
    }
}
