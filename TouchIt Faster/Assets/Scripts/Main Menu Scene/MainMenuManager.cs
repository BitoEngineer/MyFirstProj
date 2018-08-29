
using Assets.Server.Models;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Assets.Server.Protocol;

public class MainMenuManager : MonoBehaviour {

    public Button SinglePlayerButton;
    public Button MultiplayerButton;
    private bool changeToMultiplayer = false;


    // Use this for initialization
    void Start () {
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
            Authenticate();
#endif
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private void LoginReply(JsonPacket p)
    {
        if (p.ReplyStatus == ReplyStatus.OK)
        {
            PlayerInfo pi = p.DeserializeContent<PlayerInfo>();
            PlayerContainer.Instance.Info = pi;
            changeToMultiplayer = true;
        }
        else
        {
            Debug.Log("Server failed: " + p.ReplyStatus); // TODO Say to user that there's problems with the server
        }

    }

    private void Authenticated()
    {
        Debug.Log("Authentication successful");

        string userInfo = "Username: " + Social.localUser.userName +
            "\nUser ID: " + Social.localUser.id +
            "\nIsUnderage: " + Social.localUser.underage;

        Debug.Log(userInfo);

        ServerManager.Instance.Client.Start(Social.localUser.id);
        ServerManager.Instance.Client.Send(URI.Handshake, new Handshake(), LoginReply, null, 5000);
    }

    private void Authenticate()
    {
        if (Social.localUser.authenticated)
        {
            Authenticated();
        }
        else
        {
            Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
            Social.localUser.Authenticate((bool success) =>
            {
                if (success)
                {
                    Authenticated();
                }
                else
                {
                    Debug.Log("Login failed!"); // TODO Say to user that can't login with google
                }
            });
        }
    }

    private void OnDestroy()
    {
        //AdManager.Instance.DestroyBanner();
    }
}
