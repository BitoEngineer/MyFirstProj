using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using System;

public class PreloaderManager : MonoBehaviour
{

    bool t = true;

    // Use this for initialization
    void Start()
    {
        DontDestroyOnLoad(gameObject);

        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
            // enables saving game progress.
            //.EnableSavedGames()
            // registers a callback to handle game invitations received while the game is not running.
            //.WithInvitationDelegate((x, y) => Debug.Log("Invitation received"))
            // registers a callback for turn based match notifications received while the
            // game is not running.
            //.WithMatchDelegate((x, y) => Debug.Log("Match turn received"))
            // requests the email address of the player be available.
            // Will bring up a prompt for consent.
            .RequestEmail()
            // requests a server auth code be generated so it can be passed to an
            //  associated back end server application and exchanged for an OAuth token.
            .RequestServerAuthCode(false)
            // requests an ID token be generated.  This OAuth token can be used to
            //  identify the player to other services such as Firebase.
            .RequestIdToken()
            .Build();

        PlayGamesPlatform.InitializeInstance(config);
        // recommended for debugging:
        PlayGamesPlatform.DebugLogEnabled = true;
        // Activate the Google Play Games platform
        PlayGamesPlatform.Activate();

        Debug.Log("--------------------------------XXXXXXXXXXXXXXXXXXXXXXXXX-------------------------------");
    }

    // Update is called once per frame
    void Update()
    {
        if (t )
        {
            t = false;
            SceneManager.LoadScene("Main Menu");
        }
    }

    public delegate void InvitationDelegate();
    public delegate void MatchDelegate();
}
