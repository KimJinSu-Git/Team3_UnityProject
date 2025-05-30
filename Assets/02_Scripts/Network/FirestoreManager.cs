using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Firestore;
using UnityEngine;

public class FirestoreManager : MonoBehaviour
{
    public static FirestoreManager Instance;
    //public static FirestoreManager Instance => _instance ??= new FirestoreManager();

    private FirebaseApp customApp;
    public FirebaseFirestore firestore;

    private bool isInitialized = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void Start()
    {
        string appName = "CustomApp_" + Guid.NewGuid();
        await InitializeAsync(appName);
    }

    public async Task InitializeAsync(string appName)
    {
        if (isInitialized) return;

        var options = new AppOptions()
        {
            ProjectId = "teamproject-3c626",
            AppId = "466157351328",
            ApiKey = "AIzaSyCN7WoVUPt38jxVPvnXTUugT7RMFPmF20I",
        };

        customApp = FirebaseApp.Create(options, appName);
        firestore = FirebaseFirestore.GetInstance(customApp);

        isInitialized = true;
    }
}
