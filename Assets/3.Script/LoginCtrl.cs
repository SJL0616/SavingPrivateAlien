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
        if (!Social.localUser.authenticated) // �α��� �Ǿ� ���� �ʴٸ�
        {
            Social.localUser.Authenticate(success => // �α��� �õ�
            {
                if (success) // �����ϸ�
                {
                    Debug.Log("google game service Success");
                    StartCoroutine(TryFirebaseLogin()); // Firebase Login �õ�
                }
                else // �����ϸ�
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
        if (Social.localUser.authenticated) // �α��� �Ǿ� �ִٸ�
        {
            PlayGamesPlatform.Instance.SignOut(); // Google �α׾ƿ�
            auth.SignOut(); // Firebase �α׾ƿ�
        }
    }

}

