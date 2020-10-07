
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
using Newtonsoft.Json;

public class MainMenuManager : MonoBehaviour
{

    public Button SinglePlayerButton;
    public GameObject BetterThanText;
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

    public const string IP = "192.168.1.171";
    public const int PORT = 2223;

    private readonly string ClientID = "574776742495-hl8c1nhu7nkkcusmbpsmedua7a29a6g4.apps.googleusercontent.com";
    //private readonly string Jokes_API_URL = "http://geek-jokes.sameerkumar.website/api";


    // Use this for initialization
    void Start()
    {
        Debug.Log("--------------------------------ZZZZZZZZZZZZZZZZZZZZZZZZZ-------------------------------");
        Debug.Log("TouchItFaster - MainMenuManager starting");
        //SetJoke();

        if (ConnectionHelper.HasInternet)
        {
#if DEBUG
            string debugClientID = "debugtestclientid";
            ServerManager.Instance.Client.Start(IP, PORT, debugClientID, () =>
            {
                ServerManager.Instance.Client.Send(URI.Login, new Handshake(), LoginReply, null, null, null, 5000);
            });
#endif
            Debug.Log("TouchItFaster - Starting authentication");
            Authenticate();
        }
        else
        {

            var parsedPlayerInfo = PlayerPrefs.GetString(PlayerContainer.PLAYER_INFO_KEY);
            var playerInfo = JsonConvert.DeserializeObject<PlayerInfo>(parsedPlayerInfo);
            LoginReply(new JsonPacket(null, URI.Login.ToString(), playerInfo)
            {
                ReplyStatus = ReplyStatus.OK
            });
        }
    }

    private void SetJoke()
    {/*
        StartCoroutine(GetRequest(Jokes_API_URL, (joke) =>
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                JokeText.gameObject.SetActive(true);
                if (joke != null)
                {
                    JokeText.text = joke.Substring(1, joke.Length - 3);
                }
            });
        }));*/
    }

    void Update()
    {
        //AdManager.Instance.ShowBanner(); CREATE PRELOADER SCENE https://www.youtube.com/watch?v=khlROw-PfNE
    }

    public void ChangeScene(string sceneName)
    {
        Debug.Log("TouchItFaster - ChangeScene called with the scene name " + sceneName);

        if (sceneName == "Multiplayer")
        {
            while (!changeToMultiplayer) ;

        }

        SceneManager.LoadScene(sceneName);
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

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                var betterThan = pi.BetterThan <= 0 ? "-" : pi.BetterThan.ToString();
                BetterThanText.GetComponent<Text>().text = $"Hey, your best is better than {betterThan}% of users";
                BetterThanText.SetActive(true);
            });

            changeToMultiplayer = true;
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
