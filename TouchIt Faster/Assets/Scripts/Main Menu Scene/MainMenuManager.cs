
using Assets.Server.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Preloader;
using Assets.Scripts.Utils;
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
    public GameObject MessagePanel;

    /*Settings*/
    public GameObject SettingsPanel;
    public InputField NameInputField;
    public Text NameText;
    public Text WinsLosesText;
    public Text HighestScore;
    public Text TapsInARowText;

    private bool changeToMultiplayer = false;
    private bool IsSettings = false;

    public const string IP = "192.168.1.171";
    public const int PORT = 2223; 

    private readonly string ClientID = "574776742495-hl8c1nhu7nkkcusmbpsmedua7a29a6g4.apps.googleusercontent.com";
    //private readonly string Jokes_API_URL = "http://geek-jokes.sameerkumar.website/api";


    // Use this for initialization
    void Start () {

        Debug.Log("--------------------------------ZZZZZZZZZZZZZZZZZZZZZZZZZ-------------------------------");
        Debug.Log("TouchItFaster - MainMenuManager starting");
        /*StartCoroutine(GetRequest(Jokes_API_URL, (joke) =>
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    JokeText.gameObject.SetActive(true);
                    if (joke != null)
                    {
                        JokeText.text = joke.Substring(1, joke.Length-3);
                    }                        
                });
            }));*/
#if DEBUG
        string debugClientID = "debugtestclientid";
        ServerManager.Instance.Client.Start(IP, PORT, debugClientID);
#endif
        Debug.Log("TouchItFaster - MainMenuManager ended Start");
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
        Debug.Log("TouchItFaster - ChangeScene called with the scene name " + sceneName);

        if (sceneName == "Multiplayer")
        {
#if DEBUG
            ServerManager.Instance.Client.Send(URI.Login, null, LoginReply, null /* TODO*/, null, null, 50000);
#else
            Debug.Log("TouchItFaster - Starting authentication");
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
            Debug.Log("TouchItFaster - Already authenticated");
            Authenticated();
        }
        else
        {
            PlayGamesPlatform.Instance.localUser.Authenticate((bool success) =>
            {
                if (success)
                {
                    Debug.Log("TouchItFaster - Login succeded!");
                    Authenticated();
                }
                else
                {
                    Debug.Log("TouchItFaster - Login failed!"); // TODO Say to user that can't login with google
                }
            });
        }
    }
    private void Authenticated()
    {

        string userInfo = "Username: " + PlayGamesPlatform.Instance.localUser.userName +
            "\nUser ID: " + PlayGamesPlatform.Instance.localUser.id +
            "\nIsUnderage: " + PlayGamesPlatform.Instance.localUser.underage;

        Debug.Log("TouchItFaster - " + userInfo);

        ServerManager.Instance.Client.Start(IP, PORT, PlayGamesPlatform.Instance.localUser.id, () =>
        {
            ServerManager.Instance.Client.Send(URI.Login, new Handshake(), LoginReply, null, null, null, 5000);
        });
    }

    private void LoginReply(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            Debug.Log("TouchItFaster - Start with Server done");
            PlayerInfo pi = p.DeserializeContent<PlayerInfo>();
            PlayerContainer.Instance.SetPlayerInfo(pi);
            if (IsSettings)
            {
                IsSettings = false;
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    BuildSettingsPanel();
                });
            }
            else
            {
                changeToMultiplayer = true;
            }
        }
        else
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("Sh*ts happen, try again later", 3f, MessagePanel)));
            Debug.Log("TouchItFaster - Server failed: " + p.ReplyStatus); // TODO Say to user that there's problems with the server
        }

    }

    IEnumerator GetRequest(string uri, Action<string> action)
    {
        UnityWebRequest uwr = UnityWebRequest.Get(uri);
        yield return uwr.SendWebRequest();

        if (uwr.isNetworkError)
        {
            Debug.Log("TouchItFaster - Error While Sending: " + uwr.error);
            action(null);
        }
        else
        {
            Debug.Log("TouchItFaster - Received: " + uwr.downloadHandler.text);
            action(uwr.downloadHandler.text);
        }
    }

    public void BuildSettingsPanel()
    {
        NameText.text = PlayerContainer.Instance.Info.PlayerName;
        // TODO HighestScore.text = PlayerContainer.Instance.Info.HighestScore;
        // TODO TapsInARowText.text = PlayerContainer.Instance.Info.TapsInARow;
        string wltxt = WinsLosesText.text.Replace("WW", PlayerContainer.Instance.Info.Wins.ToString());
        wltxt = wltxt.Replace("LL", PlayerContainer.Instance.Info.Loses.ToString());
        WinsLosesText.text = wltxt;
        SettingsPanel.SetActive(true);
    }

    public void SettingsListener()
    {
        IsSettings = true;
        ServerManager.Instance.Client.Send(URI.Login, new Handshake(), LoginReply, null, null, null, 5000);
    }

    public void OnPlayerUpdate(JsonPacket p)
    {

    }

    private void OnDestroy()
    {
        //AdManager.Instance.DestroyBanner();
    }

    private void OnApplicationQuit()
    {
        ServerManager.Instance.Client.Stop();
    }
}
