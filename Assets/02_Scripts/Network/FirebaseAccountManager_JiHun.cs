using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class FirebaseAccountManager_JiHun : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseFirestore firestore;
    
    private string email = "";
    private string password = "";
    private string nickname = "";
    
    private string statusMessage = "";

    private bool isInitialized = false;
    private bool isLoggedIn = false;

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => // 제대로 설치되어 있는지 확인하고, 문제 있으면 고치려 시도합니다. 초기화 전에 호출해야됨
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance; // 초기화
                firestore = FirebaseFirestore.DefaultInstance;
                isInitialized = true;
                statusMessage = "Firebase 초기화 완료";
                Debug.Log(statusMessage); // 🔽 콘솔 출력
            }
            else
            {
                statusMessage = $"Firebase 초기화 실패: {task.Result}";
                Debug.Log(statusMessage); // 🔽 콘솔 출력
            }
        });
    }

    private void CreateAccount(string email, string password, string nickname)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                statusMessage = "회원가입 실패";
                return;
            }
            AuthResult result = task.Result;
            FirebaseUser newUser = result.User;
            UserManager.Instance.SetUserData(newUser);
            statusMessage = "회원가입 성공";
            Debug.Log(statusMessage);
            
            UpdateUserNickname(newUser, nickname);
            CreateUserDocument(newUser.UserId, email, nickname);
            
            //SceneManager.LoadScene("MainMenu");
        });
    }

    private void UpdateUserNickname(FirebaseUser user, string nickname)
    {
        UserProfile profile = new UserProfile
        {
            DisplayName = nickname
        };
        user.UpdateUserProfileAsync(profile).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                statusMessage += $"닉네임 설정 완료: {nickname}";
            }
            else
            {
                statusMessage += $"닉네임 설정 실패";
            }
        });
    }

    private void CreateUserDocument(string uid, string email, string nickname)
    {
        DocumentReference userDoc = firestore.Collection("users").Document(uid);
        var userData = new
        {
            email = email,
            nickname = nickname,
            createAt = Timestamp.GetCurrentTimestamp(),
            role = "user"
        };

        userDoc.SetAsync(userData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                statusMessage = "FireStore 사용자 문서 생성완료";
            }
            else
            {
                statusMessage = "FireStore 문서 생성 실패";
            }
        });
    }

    private void LogIn(string email, string password)
    {
        statusMessage = "로그인 하는 중";
        Debug.Log(statusMessage);
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                statusMessage = " 로그인 실패";
                Debug.Log("로그인실패");

                return;
            }
            AuthResult result = task.Result;
            FirebaseUser user = result.User;
            UserManager.Instance.SetUserData(user);
            isLoggedIn = true;
            Debug.Log("로그인성공");
            statusMessage = " 로그인 성공";
            SceneManager.LoadScene("MainMenu");
        });
    }
    
    [SerializeField] private TMPro.TextMeshProUGUI login_IdText;
    [SerializeField] private TMPro.TextMeshProUGUI login_PasswordText;
    public void OnLogin()
    {
        email = login_IdText.text.Trim();
        password = login_PasswordText.text.Trim();;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            statusMessage = "모든 정보를 입력해주세요.";
            return;

        }else if (isInitialized.Equals(false))
        {
            statusMessage = "잠시후, 다시 입력해주세요";
            return;
        }
        else
        {
            LogIn(email, password);
        }
        Debug.Log(statusMessage);
    }

    [SerializeField] private TMPro.TextMeshProUGUI signin_IdText;
    [SerializeField] private TMPro.TextMeshProUGUI signin_PasswordText;
    [SerializeField] private TMPro.TextMeshProUGUI signin_NickNameText;
    
    public void OnSignUp() //DrawSignUpUI(float centerX, float centerY)
    {
        email = signin_IdText.text.Trim();;
        password = signin_PasswordText.text.Trim();
        nickname = signin_NickNameText.text.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(nickname))
        {
            statusMessage = "모든 정보를 입력해주세요.";
            return;

        }
        
        if (isInitialized.Equals(false))
        {
            statusMessage = "잠시후, 다시 입력해주세요";
        }
        else
        {
            CreateAccount(email, password, nickname);
        }
    }
    
    private void SignOut()
    {
        auth.SignOut();
        isLoggedIn = false;
        statusMessage = "로그아웃";
        UserManager.Instance.SetUserData(null);
    }
}
