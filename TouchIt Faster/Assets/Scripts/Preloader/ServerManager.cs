using Assets.Server.Models;
using Assets.Server.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ServerManager : MonoBehaviour {

    public static ServerManager Instance { set; get; }

    public readonly MyServClient Client = new MyServClient("192.168.1.70", 2222);

    // Use this for initialization
    void Start () {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Client.OnConnectivityChange += connectivityChanged;
        Client.LogDebugEvent += logDebug;
        Client.LogErrorEvent += errorDebug;
    }

    private void errorDebug(Exception e, string message, object[] args)
    {
        Console.WriteLine("Preloader ERROR: "+ string.Format(message, args));
        Console.WriteLine("Preloader ERROR: " + e);
    }

    private void logDebug(string message, object[] args)
    {
        Console.WriteLine("Preloader: " + string.Format(message, args));
    }

    private void connectivityChanged(bool connected)
    {
        if (!connected)
        {
            // TODO - connection to server lost
        }
        else
        {
            Console.WriteLine("Preloader INFO: Connected to server");
        }
    }
}
