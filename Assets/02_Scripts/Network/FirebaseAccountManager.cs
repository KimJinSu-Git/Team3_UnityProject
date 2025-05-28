using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class FirebaseAccountManager : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseFirestore firestore;
    
    private string email = "";
    private string password = "";
    private string nickname = "";
    
    private string statusMessage = "";

    private bool isInitialized = false;
    private bool isLoggedIn = false;
    private bool isSignUpMode = false;

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
            DisplayName = this.nickname
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

    private void SignIn(string email, string password)
    {
        statusMessage = "로그인 하는 중";
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

    private void SignOut()
    {
        auth.SignOut();
        isLoggedIn = false;
        statusMessage = "로그아웃";
        UserManager.Instance.SetUserData(null);
    }
    private void OnGUI()
    {
        float centerX = Screen.width / 2;
        float centerY = Screen.height / 2;
    
        GUI.Box(new Rect(centerX - 200, centerY - 100, 400, 200), "");
    
        if (!isInitialized)
        {
            GUI.Label(new Rect(10, 10, 500, 30), "Firebase 초기화 중...");
            return;
        }
    
        GUI.Label(new Rect(10, 10, 500, 25), statusMessage);
        if (isSignUpMode) DrawSignUpUI(centerX, centerY);
        else DrawLoginUI(centerX, centerY);
    }
    private void DrawLoginUI(float centerX, float centerY)
    {
        GUI.Label(new Rect(centerX - 160, centerY - 40, 100, 25), "Email:");
        email = GUI.TextField(new Rect(centerX - 50, centerY - 40, 200, 25), email);
    
        GUI.Label(new Rect(centerX - 160, centerY, 100, 25), "Password:");
        password = GUI.PasswordField(new Rect(centerX - 50, centerY, 200, 25), password, '*');
    
        if (GUI.Button(new Rect(centerX - 150, centerY + 50, 150, 30), "로그인"))
        {
            SignIn(email, password); // ******************* 이것만 버튼에 연동 *******************
        }
    
        if (GUI.Button(new Rect(centerX + 10, centerY + 50, 150, 30), "회원가입"))
        {
            isSignUpMode = true;
            statusMessage = "회원가입 화면으로 전환됨";
        }
    }
    private void DrawSignUpUI(float centerX, float centerY)
    {
        GUI.Label(new Rect(centerX - 160, centerY - 60, 100, 25), "Email:");
        email = GUI.TextField(new Rect(centerX - 50, centerY - 60, 200, 25), email);
    
        GUI.Label(new Rect(centerX - 160, centerY - 20, 100, 25), "Password:");
        password = GUI.PasswordField(new Rect(centerX - 50, centerY - 20, 200, 25), password, '*');
    
        GUI.Label(new Rect(centerX - 160, centerY + 20, 100, 25), "Nickname:");
        nickname = GUI.TextField(new Rect(centerX - 50, centerY + 20, 200, 25), nickname);
    
        if (GUI.Button(new Rect(centerX - 150, centerY + 70, 150, 30), "회원가입"))
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(nickname))
            {
                statusMessage = "모든 정보를 입력해주세요.";
                return;
            }
    
            CreateAccount(email, password, nickname); // ******************* 이것만 버튼에 연동 *******************
        }
    
        if (GUI.Button(new Rect(centerX + 10, centerY + 70, 150, 30), "뒤로"))
        {
            isSignUpMode = false;
            statusMessage = "로그인 화면으로 전환됨";
        }
    }
    
}
