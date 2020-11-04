using Assets.Scripts.Main_Menu_Scene;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusicScript : MonoBehaviour
{
    private AudioSource _audioSource;
    private void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        _audioSource = GetComponent<AudioSource>();
        SoundsManager.Start();

        if (SoundsManager.IsVolumeOn)
        {
            PlayMusic();
        }
        else
        {
            StopMusic();
        }
    }

    public void PlayMusic()
    {
        if (_audioSource.isPlaying) return;
        _audioSource.Play(); //Release 
    }

    public void StopMusic()
    {
        _audioSource.Stop();
    }
}
