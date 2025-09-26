using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Fusion;
using UnityEngine;

public class UserManager : MonoBehaviour
{
    public static UserManager Instance;

    public FirebaseUser userData;
    public PlayerRef FusionPlayerRef;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetUserData(FirebaseUser user)
    {
        userData = user;
        Debug.Log($"userName ::: {userData.DisplayName}");
    }

    public void SetFusionPlayerRef(PlayerRef playerRef)
    {
        FusionPlayerRef = playerRef;
    }
}
