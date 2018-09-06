using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Utils
{
    public static class UIUtils
    {
        public static IEnumerator ShowMessageInPanel(string message, float delay, GameObject MessagePanel)
        {
            MessagePanel.SetActive(true);
            MessagePanel.GetComponentInChildren<Text>().text = message;
            yield return new WaitForSeconds(delay);
            MessagePanel.SetActive(false);
        }

        public static IEnumerator ShowMessageInText(string message, float delay, Text MessageText)
        {
            MessageText.gameObject.SetActive(true);
            MessageText.text = message;
            yield return new WaitForSeconds(delay);
            MessageText.gameObject.SetActive(false);
        }
    }
}
