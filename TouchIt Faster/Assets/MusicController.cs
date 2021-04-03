using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    public AudioClip OnButtonClickNegativeAudio;
    public AudioClip OnButtonClickAudio;

    private AudioSource source;
    void Start()
    {
        DontDestroyOnLoad(transform.gameObject);
        source = GetComponent<AudioSource>();
    }

    public void PlayOnButtonClickAudio()
    {
        source.clip = OnButtonClickAudio;
        source.Play();
    }

    public void PlayOnButtonClickNegativeAudio()
    {
        source.clip = OnButtonClickNegativeAudio;
        source.Play();;
    }
}
