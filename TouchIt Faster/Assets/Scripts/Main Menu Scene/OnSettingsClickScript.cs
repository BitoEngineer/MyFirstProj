using Assets.Scripts.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnSettingsClickScript : MonoBehaviour
{

    private GameObject VolumeButton => gameObject.transform.Find("VolumeButton").gameObject;
    private GameObject NoVolumeButton => gameObject.transform.Find("NoVolumeButton").gameObject;
    private GameObject LikeButton => gameObject.transform.Find("LikeButton").gameObject;
    private GameObject BugButton => gameObject.transform.Find("BugButton").gameObject;
    private GameObject InfoButton => gameObject.transform.Find("InfoButton").gameObject;

    public void OnSettingsClick()
    {
        if (InfoButton.activeSelf)
        {
            HideSettingsLayout();
        }
        else
        {
            RenderSettingsLayout();
        }
    }

    private void HideSettingsLayout()
    {
        VolumeButton.SetActive(false);
        NoVolumeButton.SetActive(false);
        LikeButton.SetActive(false);
        BugButton.SetActive(false);
        InfoButton.SetActive(false);
    }

    private void RenderSettingsLayout()
    {
        int sound = PlayerPrefs.GetInt(PlayerPrefsKeys.SOUND_ON);
        bool isSoundOn = sound >= 0; //TODO create SoundManager class and propagate 

        VolumeButton.SetActive(!isSoundOn);
        NoVolumeButton.SetActive(isSoundOn);
        LikeButton.SetActive(true);
        BugButton.SetActive(true);
        InfoButton.SetActive(true);
    }

    public void OnVolumeClick()
    {
        //TODO
    }

    public void OnNoVolumeClick()
    {
        //TODO
    }

    public void OnBugClick()
    {
        //TODO
    }

    public void OnInfoClick()
    {
        //TODO show instagram, email, sonia, tresh, etc
    }

    public void OnLikeClick()
    {
        //TODO redirect to play store
    }
}
