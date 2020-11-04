using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Main_Menu_Scene
{
    public static class SoundsManager
    {
        public static bool IsVolumeOn = false;


        /// <summary>
        /// MUST BE CALLED IN THE UI THREAD 
        /// </summary>
        public static void Start()
        {
            int sound = PlayerPrefs.GetInt(PlayerPrefsKeys.SOUND_ON);
            IsVolumeOn = sound >= 0;
        }

        /// <summary>
        /// MUST BE CALLED IN THE UI THREAD 
        /// </summary>
        public static void SetVolumeOff()
        {
            IsVolumeOn = false;
            PlayerPrefs.SetInt(PlayerPrefsKeys.SOUND_ON, -1);

            var backgroundMusic = GameObject.FindGameObjectWithTag("BackgroundMusic");
            backgroundMusic?.GetComponent<BackgroundMusicScript>()?.StopMusic();
        }

        /// <summary>
        /// MUST BE CALLED IN THE UI THREAD 
        /// </summary>
        public static void SetVolumeOn()
        {
            IsVolumeOn = true;
            PlayerPrefs.SetInt(PlayerPrefsKeys.SOUND_ON, 1);

            var backgroundMusic = GameObject.FindGameObjectWithTag("BackgroundMusic");
            backgroundMusic?.GetComponent<BackgroundMusicScript>()?.PlayMusic();
        }
    }
}
