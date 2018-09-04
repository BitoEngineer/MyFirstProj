﻿
using Assets.Server.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Preloader;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Assets.Server.Protocol;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.Networking;

public class MainMenuManager : MonoBehaviour {

    public Button SinglePlayerButton;
    public Button MultiplayerButton;
    public Text JokeText;

    private bool changeToMultiplayer = false;

    private readonly string ClientID = "574776742495-hkkjt0av75rdb3ipceh6iefugrikuldm.apps.googleusercontent.com";
    private readonly string Jokes_API_URL = "http://geek-jokes.sameerkumar.website/api";


    // Use this for initialization
    void Start () {
        StartCoroutine(GetRequest(Jokes_API_URL, (joke) =>
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    JokeText.gameObject.SetActive(true);
                    if (joke != null)
                    {
                        JokeText.text = joke.Substring(1, joke.Length-2);
                    }                        
                });
            }));
#if DEBUG
        string debugClientID = "debugtestclientid";
        ServerManager.Instance.Client.Start(debugClientID);
#endif
    }

    // Update is called once per frame
    void Update () {
        //AdManager.Instance.ShowBanner(); CREATE PRELOADER SCENE https://www.youtube.com/watch?v=khlROw-PfNE
        if (changeToMultiplayer)
        {
            changeToMultiplayer = false;
            SceneManager.LoadScene("Multiplayer");
        }

    }

    public void ChangeScene(string sceneName)
    {
        if (sceneName == "Multiplayer")
        {
#if DEBUG
            ServerManager.Instance.Client.Send(URI.Login, null, LoginReply, null /* TODO*/, 50000);
#else
            Debug.Log("Starting authentication");
            Authenticate();
#endif
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private void Authenticate()
    {
        if (PlayGamesPlatform.Instance.localUser.authenticated)
        {
            Debug.Log("Already authenticated");
            Authenticated();
        }
        else
        {
            PlayGamesPlatform.Instance.localUser.Authenticate((bool success) =>
            {
                if (success)
                {
                    Debug.Log("Login succeded!");
                    Authenticated();
                }
                else
                {
                    Debug.Log("Login failed!"); // TODO Say to user that can't login with google
                }
            });
        }
    }
    private void Authenticated()
    {

        string userInfo = "Username: " + PlayGamesPlatform.Instance.localUser.userName +
            "\nUser ID: " + PlayGamesPlatform.Instance.localUser.id +
            "\nIsUnderage: " + PlayGamesPlatform.Instance.localUser.underage;

        Debug.Log(userInfo);

        ServerManager.Instance.Client.Start(PlayGamesPlatform.Instance.localUser.id);
        ServerManager.Instance.Client.Send(URI.Handshake, new Handshake(), LoginReply, null, 5000);

    }

    private void LoginReply(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            Debug.Log("Start with Server done");
            PlayerInfo pi = p.DeserializeContent<PlayerInfo>();
            PlayerContainer.Instance.Info = pi;
            changeToMultiplayer = true;
        }
        else
        {
            Debug.Log("Server failed: " + p.ReplyStatus); // TODO Say to user that there's problems with the server
        }

    }

    IEnumerator GetRequest(string uri, Action<string> action)
    {
        UnityWebRequest uwr = UnityWebRequest.Get(uri);
        yield return uwr.SendWebRequest();

        if (uwr.isNetworkError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
            action(null);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);
            action(uwr.downloadHandler.text);
        }
    }

    private void OnDestroy()
    {
        //AdManager.Instance.DestroyBanner();
    }
}
