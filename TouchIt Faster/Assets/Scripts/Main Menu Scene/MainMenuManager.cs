
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

    private static bool isAlreadyLoggedIn = false;

    public const string IP = "192.168.1.171";////
    //public const string IP = "touchitfaster.westeurope.cloudapp.azure.com";//"40.115.22.55";// 
    public const int PORT = 2222;

    private readonly string ClientID = "574776742495-hl8c1nhu7nkkcusmbpsmedua7a29a6g4.apps.googleusercontent.com";
    //private readonly string Jokes_API_URL = "http://geek-jokes.sameerkumar.website/api";

    private int singlePlayerHighestScore = 0;
    private int singlePlayerTapsInRow = 0;

    // Use this for initialization
    void Start()
    {
        singlePlayerHighestScore = (int)PlayerPrefs.GetFloat(PlayerPrefsKeys.SINGLE_HIGHEST_SCORE_KEY);
        singlePlayerTapsInRow = (int)PlayerPrefs.GetFloat(PlayerPrefsKeys.SINGLE_MAX_TAPS_KEY);

        GameObject.FindGameObjectWithTag("BackgroundMusic")?.GetComponent<BackgroundMusicScript>()?.PlayMusic();

        Debug.Log("--------------------------------ZZZZZZZZZZZZZZZZZZZZZZZZZ-------------------------------");
        Debug.Log("TouchItFaster - MainMenuManager starting");
        //SetJoke();


        if (ConnectionHelper.HasInternet)
        {
#if DEBUG
            string debugClientID = "testingfirstlogin-1";
            ConnectToServerAndSendLogin(debugClientID);
            Debug.Log("TouchItFaster - Starting authentication");
#else
            if (!isAlreadyLoggedIn)
            {
                var clientId = PlayerPrefs.GetString("touchitfaster-clientid");

                if (string.IsNullOrEmpty(clientId))
                {
                    AuthenticateWithGoogle();
                    //LoginWithoutGoogle();
                }
                else
                {
                    ConnectToServerAndSendLogin(clientId);
                }
            }
#endif
        }
        else
        {

            var parsedPlayerInfo = PlayerPrefs.GetString(PlayerPrefsKeys.PLAYER_INFO_KEY);
            var playerInfo = JsonConvert.DeserializeObject<PlayerInfo>(parsedPlayerInfo);
            OnLogin(new JsonPacket(null, URI.Login.ToString(), playerInfo)
            {
                ReplyStatus = ReplyStatus.OK
            });
        }
    }

    private void LoginWithoutGoogle()
    {
        var clientId = PlayerPrefs.GetString("clientId");
        var isFirstLogin = string.IsNullOrEmpty(clientId);
        if (isFirstLogin)
        {
            var newUserClientId = Guid.NewGuid().ToString().Substring(0, 12);
            ConnectToServerAndSendLogin(newUserClientId);
            PlayerPrefs.SetString("clientId", newUserClientId);
        }
        else
        {
            ConnectToServerAndSendLogin(clientId);
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

        if (sceneName == "Multiplayer" && !isAlreadyLoggedIn)
        {
            var errorMessage = "F*ck, something really bad hapened :O";
            Debug.Log(errorMessage);
            UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel(errorMessage, 2f, MessagePanel)));
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    private void AuthenticateWithGoogle()
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
                    UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UIUtils.ShowMessageInPanel("Google Sign In didn't complete with success", 2f, MessagePanel)));
                    Debug.Log("TouchItFaster - Login failed!"); 
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

        ConnectToServerAndSendLogin(PlayGamesPlatform.Instance.localUser.id);
    }

    private void ConnectToServerAndSendLogin(string clientId)
    {
        MultiplayerButton.GetComponent<Button>().interactable = false;
        //Google Client Id: PlayGamesPlatform.Instance.localUser.id
        ServerManager.Instance.Client.Start(IP, PORT, clientId, () =>
        {
            var loginDto = new SinglePlayerStatsDto()
            {
                MaxHitsInRowSinglePlayer = singlePlayerTapsInRow,
                SinglePlayerHighestScore = singlePlayerHighestScore
            };

            ServerManager.Instance.Client.Send(URI.Login, loginDto, OnLogin, null, null, null, 5000);
        });
    }

    private void OnLogin(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            isAlreadyLoggedIn = true;

            Debug.Log("TouchItFaster - Start with Server done");
            PlayerInfo pi = p.DeserializeContent<PlayerInfo>();
            MyPlayer.Instance.SetPlayerInfo(pi);

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                MultiplayerButton.GetComponent<Button>().interactable = true;

                var clientId = ServerManager.Instance.Client.ClientId;
                PlayerPrefs.SetString("touchitfaster-clientid", clientId);

                var betterThan = pi.BetterThan <= 0 ? "-" : pi.BetterThan.ToString();
                BetterThanText.GetComponent<Text>().text = $"Hey, your best is better than {betterThan}% of users";
                BetterThanText.SetActive(true);
            });
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
