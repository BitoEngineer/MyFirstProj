﻿using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SquareClick : MonoBehaviour {

    private TouchManager touchM;
    public AudioMixerGroup CircleAudioMixer;

    //Use this for initialization
    void Start()
    {
        touchM = GameObject.Find("Touch").GetComponent<TouchManager>();
    }

    private void PlaySound()
    {
        var source = GetComponent<AudioSource>();

        source.outputAudioMixerGroup = CircleAudioMixer;
        source.PlayOneShot(source.clip);
    }

    private bool isDead = false;
    private void OnMouseDown()
    {
        if (!touchM.OnPause && !isDead)
        {
            PlaySound();

            touchM.SquareTouched(gameObject);

            isDead = true;
            gameObject.GetComponent<SpriteRenderer>().enabled = false;

            Task.Delay(3000).ContinueWith(t =>
            {
                Destroy(gameObject);
            });
        }
    }
}
