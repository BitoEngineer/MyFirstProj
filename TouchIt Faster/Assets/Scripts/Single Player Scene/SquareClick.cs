using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SquareClick : MonoBehaviour {

    private TouchManager touchM;

    //Use this for initialization
    void Start()
    {
        touchM = GameObject.Find("Touch").GetComponent<TouchManager>();
    }

    private void PlaySound()
    {
        var audioSource = GetComponent<AudioSource>();
        AudioSource.PlayClipAtPoint(audioSource.clip, transform.position, 1f);
    }

    private void OnMouseDown()
    {
        if (!touchM.OnPause)
        {
            PlaySound();

            touchM.SquareTouched(gameObject);
            Destroy(gameObject);
        }
    }
}
