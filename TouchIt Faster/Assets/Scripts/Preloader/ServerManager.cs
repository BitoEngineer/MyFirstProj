using Assets.Server.Models;
using Assets.Server.Protocol;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerManager : MonoBehaviour {

    public static ServerManager Instance { set; get; }

    public readonly MyServClient Client = new MyServClient("192.168.1.160", 2223);// new MyServClient("MyFirstDomain.cloudapp.net", 2222);

    public string NextScene { get; set; }

    // Use this for initialization
    void Start ()
    {
        Debug.Log("TouchItFaster - ServerManager createad");

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Client.OnConnectivityChange += connectivityChanged;
        Client.LogDebugEvent += logDebug;
        Client.LogErrorEvent += errorDebug;
    }

    private void Update()
    {
        if (!string.IsNullOrEmpty(NextScene))
        {
            string scene = NextScene;
            NextScene = null;
            ChangeScene(scene);
        }
    }

    private void errorDebug(Exception e, string message, object[] args)
    {
        Debug.Log("TouchIt Faster - Preloader ERROR: "+ string.Format(message, args));
        Debug.Log("TouchIt Faster - Preloader ERROR: " + e);
    }

    private void logDebug(string message, object[] args)
    {
        Debug.Log("TouchIt Faster - Preloader: " + string.Format(message, args));
    }

    private void connectivityChanged(bool connected)
    {
        Debug.Log("TouchItFaster - ServerManager connectivity changed");
        if (!connected)
        {
            // TODO - connection to server lost
        }
        else
        {
            Debug.Log("TouchIt Faster - Preloader INFO: Connected to server");
        }
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
