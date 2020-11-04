using Assets.Scripts.Main_Menu_Scene;
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
        RenderVolumeIcon();
        LikeButton.SetActive(true);
        BugButton.SetActive(true);
        InfoButton.SetActive(true);
    }

    public void SetVolumeOff()
    {
        SoundsManager.SetVolumeOff();
        RenderVolumeIcon();
    }

    public void SetVolumeOn()
    {
        SoundsManager.SetVolumeOn();
        RenderVolumeIcon();
    }

    public void OnLikeClick()
    {
        //TODO redirect to play store
    }

    private void RenderVolumeIcon()
    {
        VolumeButton.SetActive(SoundsManager.IsVolumeOn);
        NoVolumeButton.SetActive(!SoundsManager.IsVolumeOn);
    }
}
