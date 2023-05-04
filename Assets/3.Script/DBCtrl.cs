using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;


using Firebase.Extensions;
public class User
{

    public string userId;
    public string createdDate;
    public string lastLoginDate;
    public int clearStage;
    public int score;

    public User(string _userId)
    {
        this.userId = _userId;
        this.createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        this.lastLoginDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        this.clearStage = 0;
        this.score = 0;
    }
}

public class DBCtrl: MonoBehaviour
{
    DatabaseReference m_Reference;
    string userId = string.Empty;
    public UIManager uIManager;


    void Start()
    {
        m_Reference = FirebaseDatabase.DefaultInstance.RootReference;

        //유니티 테스트용
#if UNITY_EDITOR_WIN
        ReadUserData("score", GameManager.Instance.ShowScore);
        ReadUserData("clearStage", uIManager.InitStageNum);
#endif
    }

    public void WriteUserData(string category ,string value)
    {
        if(userId != String.Empty)
        {
            //아래 코드는 테스트용
#if UNITY_EDITOR_WIN
            m_Reference.Child("users").Child("yJQRG6uTPJZD7o9tOBmb6SYRCQr2").Child(category).SetValueAsync(value);

#else
            m_Reference.Child("users").Child(userId).Child(category).SetValueAsync(value);
#endif
        }
    }

    public string ReadUserData(string category, Action<string>  callback)
    {
        string data = string.Empty;
        DataSnapshot snapshot = null;
        FirebaseDatabase.DefaultInstance.GetReference("users")
           .GetValueAsync().ContinueWithOnMainThread(task =>
           {
               if (task.IsFaulted)
               {
                   callback(string.Empty);
               }
               else if (task.IsCompleted)
               {
                   snapshot = task.Result;
                   string val = string.Empty;

#if UNITY_EDITOR_WIN
                   if (snapshot.Child("yJQRG6uTPJZD7o9tOBmb6SYRCQr2").HasChild(category))
                   {
                       val = snapshot.Child("yJQRG6uTPJZD7o9tOBmb6SYRCQr2").Child(category).Value.ToString();
                   }
#else
                   if (snapshot.Child(userId).HasChild(category))
                   {
                       val = snapshot.Child(userId).Child(category).Value.ToString();
                   }
#endif
                   callback(val);
               }
           });
        return data;
    }

    public void InitDataBase(string _userId)
    {
        userId = _userId;
        FirebaseDatabase.DefaultInstance.GetReference("users")
         .GetValueAsync().ContinueWithOnMainThread(task =>
         {
             if (task.IsFaulted)
             {
                    // Handle the error...
             }
             else if (task.IsCompleted)
             {
            
                 DataSnapshot snapshot = task.Result;
                 if (snapshot.HasChild(userId))
                 {
                     m_Reference.Child("users").Child(userId).Child("lastLoginDate").SetValueAsync(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                 }
                 else
                 {
                     User user = new User(userId);
                     string json = JsonUtility.ToJson(user);
                     m_Reference.Child("users").Child(userId).SetRawJsonValueAsync(json);
                 }             
             }
         });
        ReadUserData("clearStage", uIManager.InitStageNum);
        ReadUserData("score", GameManager.Instance.ShowScore);
    }
}