using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames.BasicApi;
using GooglePlayGames;
using Firebase.Auth;

public class LoginCtrl : MonoBehaviour
{
    private FirebaseAuth auth;
    public string FireBaseId = string.Empty;

    public static LoginCtrl Instance = null;
    private DBCtrl db;
    private void Awake()
    {
        db = GetComponent<DBCtrl>();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartLOGIN();
    }

    //void Login()
    //{
    //    if (PlayGamesPlatform.Instance.localUser.authenticated == false)
    //    {
    //        Social.localUser.Authenticate((bool success) => {
    //            if (success) google.text = Social.localUser.id + " , " + Social.localUser.userName;
    //            else google.text = "Faild";
    //        });
    //    }
    //}


    void StartLOGIN()
    {
        var config = new PlayGamesClientConfiguration.Builder()
            .RequestServerAuthCode(false)
            .AddOauthScope("profile")
            .RequestIdToken()
            .Build();

        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        auth = FirebaseAuth.DefaultInstance;

        Instance = this;
        Login();
    }



    public void Login()
    {
        if (!Social.localUser.authenticated) // 로그인 되어 있지 않다면
        {
            Social.localUser.Authenticate(success => // 로그인 시도
            {
                if (success) // 성공하면
                {
                    Debug.Log("google game service Success");
                    StartCoroutine(TryFirebaseLogin()); // Firebase Login 시도
                }
                else // 실패하면
                {
                    Debug.Log("google game service Fail");
                }
            });
        }
    }

    IEnumerator TryFirebaseLogin()
    {
        while (string.IsNullOrEmpty(((PlayGamesLocalUser)Social.localUser).GetIdToken()))
        {
            yield return null;
        }

        string idToken = ((PlayGamesLocalUser)Social.localUser).GetIdToken();


        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
        auth.SignInWithCredentialAsync(credential).ContinueWith(task => {
            if (task.IsCanceled)
            {
                return;
            }
            if (task.IsFaulted)
            {
                return;
            }

            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                        newUser.DisplayName, newUser.UserId);
            db.InitDataBase(newUser.UserId);

        });
    }



    public void TryGoogleLogout()
    {
        if (Social.localUser.authenticated) // 로그인 되어 있다면
        {
            PlayGamesPlatform.Instance.SignOut(); // Google 로그아웃
            auth.SignOut(); // Firebase 로그아웃
        }
    }

}

