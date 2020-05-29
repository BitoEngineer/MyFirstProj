using Firebase;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PreloaderManager : MonoBehaviour
{

    bool t = true;
    public FirebaseApp app;

    // Use this for initialization
    void Start()
    {
        Debug.Log("TouchItFaster - PreloaderManager createad");

        DontDestroyOnLoad(gameObject);

        CheckIfGooglePlayServicesIsUpToDate();
        PlayGamesPlatformConfig();

        Debug.Log("--------------------------------XXXXXXXXXXXXXXXXXXXXXXXXX-------------------------------");
    }

    private void PlayGamesPlatformConfig()
    {
        Debug.Log("TouchItFaster - Starting PlayGamesPlatformConfig");
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
                    //.RequestServerAuthCode(false)
                    // requests an ID token be generated.  This OAuth token can be used to
                    //  identify the player to other services such as Firebase.
                    .RequestIdToken()
                    .Build();

        PlayGamesPlatform.InitializeInstance(config);
        // recommended for debugging:
        PlayGamesPlatform.DebugLogEnabled = true;
        // Activate the Google Play Games platform
        PlayGamesPlatform.Activate();

        Debug.Log("TouchItFaster - Ended PlayGamesPlatformConfig");
    }

    private void CheckIfGooglePlayServicesIsUpToDate()
    {
        Debug.Log("TouchItFaster - Before CheckIfGooglePlayServicesIsUpToDate");
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                Debug.Log("TouchItFaster - CheckIfGooglePlayServicesIsUpToDate - dependency status is available");
                // Create and hold a reference to your FirebaseApp, i.e.
                app = Firebase.FirebaseApp.DefaultInstance;
                // where app is a Firebase.FirebaseApp property of your application class.

                // Set a flag here indicating that Firebase is ready to use by your
                // application.
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "TouchItFaster - Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (t)
        {
            Debug.Log("--------------------------------YYYYYYYYYYYYYYYYYYYYYYYYY-------------------------------");
            Debug.Log("TouchItFaster - PreloaderManager will load Main Menu scene");
            t = false;
            SceneManager.LoadScene("Main Menu");
            Debug.Log("TouchItFaster - PreloaderManager after calling LoadScene(Main Menu)");
        }
    }

    public delegate void InvitationDelegate();
    public delegate void MatchDelegate();
}
