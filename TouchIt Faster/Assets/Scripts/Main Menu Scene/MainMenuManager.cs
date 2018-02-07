
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
            string debugClientID = "debugtestclientid";
            ServerManager.Instance.SetClientID(debugClientID);
            ServerManager.Instance.Send((byte)TouchItFasterContentType.Handshake, new Handshake(), HandshakeReply, 5000);
#else
            Authenticate();
#endif
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private void HandshakeReply(JsonPacket p, ServerManager.ReplyResult result)
    {
        if(result == ServerManager.ReplyResult.Success)
        {
            if (p.ContentType == (byte)TouchItFasterContentType.PlayerInfo)
            {
                PlayerInfo pi = p.DeserializeContent<PlayerInfo>();
                PlayerContainer.Instance.Info = pi;
                changeToMultiplayer = true;
            }
            else
            {
                Debug.Log("Unexpected Content Type: " + p.ContentType);
            }         
        }
        else
        {
            Debug.Log("Server failed: " + result); // TODO Say to user that there's problems with the server
        }

    }

    private void Authenticated()
    {
        Debug.Log("Authentication successful");

        string userInfo = "Username: " + Social.localUser.userName +
            "\nUser ID: " + Social.localUser.id +
            "\nIsUnderage: " + Social.localUser.underage;

        Debug.Log(userInfo);

        ServerManager.Instance.SetClientID(Social.localUser.id);
        ServerManager.Instance.Send((byte)TouchItFasterContentType.Handshake, new Handshake(), HandshakeReply, 5000);
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
