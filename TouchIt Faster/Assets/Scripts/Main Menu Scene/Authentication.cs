using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using UnityEngine.Experimental.UIElements;
using GooglePlayGames.BasicApi;

public class Authentication : MonoBehaviour {

    Firebase.Auth.FirebaseAuth auth;
    public string googleIdToken = "", googleAccessToken = "";
    public string ClientID = "574776742495-hkkjt0av75rdb3ipceh6iefugrikuldm.apps.googleusercontent.com";


    // Use this for initialization
    void Start () {
        // Configure sign-in to request the user's ID, email address, and basic
        // profile. ID and basic profile are included in DEFAULT_SIGN_IN.
        //GoogleSignInOptions gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DEFAULT_SIGN_IN)
        //       .requestEmail()
        //        .build();
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnLoginClick()
    {
        if (!Social.localUser.authenticated)
        {
            Social.localUser.Authenticate((bool success) =>
            {
                if (success)
                {
                    Debug.Log("Authentication successful");
                    ((PlayGamesPlatform)Social.Active).SetGravityForPopups(Gravity.TOP);
                    string userInfo = "Username: " + Social.localUser.userName +
                        "\nUser ID: " + Social.localUser.id +
                        "\nIsUnderage: " + Social.localUser.underage;



                    Debug.Log(userInfo);
                }

                else
                {
                    Debug.Log("Login failed!");
                }
            });
        }
        
        //AuthenticateUser();

    }


    public void AuthenticateUser()
    {

        Firebase.Auth.Credential credential =
        Firebase.Auth.GoogleAuthProvider.GetCredential(googleIdToken, null);
        auth.SignInWithCredentialAsync(credential).ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithCredentialAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                return;
            }

            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
        });


    }

    public void UserInformation()
    {
        Firebase.Auth.FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            string name = user.DisplayName;
            string email = user.Email;
            System.Uri photo_url = user.PhotoUrl;
            // The user's Id, unique to the Firebase project.
            // Do NOT use this value to authenticate with your backend server, if you
            // have one; use User.TokenAsync() instead.
            string uid = user.UserId;
        }
    }

    public void DisconnectUser()
    {
        auth.SignOut();
    }
}
