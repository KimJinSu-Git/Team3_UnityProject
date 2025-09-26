using System;
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
    //private FirebaseFirestore firestore;
    
    private string email = "";
    private string password = "";
    private string nickname = "";
    
    private string statusMessage = "";

    private bool isInitialized = false;
    private bool isLoggedIn = false;
    private bool isSignUpMode = false;
    // [SerializeField] private TextMeshProUGUI id_Text;
    // [SerializeField] private TextMeshProUGUI password_Text;
    // 로그인 창
    [SerializeField] private TMP_InputField inputField_Id;
    [SerializeField] private TMP_InputField inputField_Password;
        
    // 회원가입 창    
    [SerializeField] private TMP_InputField inputField_SignId;
    [SerializeField] private TMP_InputField inputField_SignPassword;
    [SerializeField] private TMP_InputField inputField_SignNickname;
    
    // public TMP_InputField inputField;
    
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject signUpPanel;

    public void SwitchToSignUp()
    {
        loginPanel.SetActive(false);
        signUpPanel.SetActive(true);
    }

    public void SwitchToLogin()
    {
        loginPanel.SetActive(true);
        signUpPanel.SetActive(false);
    }
    
    public void OnClickSignUp()
    {
        string email = inputField_SignId.text;
        string password = inputField_SignPassword.text;
        string nickname = inputField_SignNickname.text;

        CreateAccount(email, password, nickname);
    }
    
    public void OnClickLogin()
    {
        string email = inputField_Id.text;
        string password = inputField_Password.text;

        SignIn(email, password);
    }
    // public void OnInputValueChanged()
    // {
    //     string id_Text = inputField.text;
    // }
    
    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => // 제대로 설치되어 있는지 확인하고, 문제 있으면 고치려 시도합니다. 초기화 전에 호출해야됨
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance; // 초기화
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
        DocumentReference userDoc = FirestoreManager.Instance.firestore.Collection("users").Document(uid);
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
                
                Debug.LogError(task.Exception.Message);

                return;
            }
            AuthResult result = task.Result;
            FirebaseUser user = result.User;
            UserManager.Instance.SetUserData(user);
            isLoggedIn = true;
            Debug.Log("로그인성공");
            statusMessage = " 로그인 성공";
            SceneManager.LoadScene("MainMenu_New");
        });
    }

    private void SignOut()
    {
        auth.SignOut();
        isLoggedIn = false;
        statusMessage = "로그아웃";
        UserManager.Instance.SetUserData(null);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            HandleTabFocus();
        }
    }
    
    private void HandleTabFocus()
    {
        if (inputField_Id.isFocused)
        {
            inputField_Password.Select();
        }
        else if (inputField_Password.isFocused)
        {
            inputField_Id.Select();
        }
        else if (inputField_SignId != null && inputField_SignId.isFocused)
        {
            inputField_SignPassword.Select();
        }
        else if (inputField_SignPassword != null && inputField_SignPassword.isFocused)
        {
            inputField_SignNickname.Select();
        }
        else if (inputField_SignNickname != null && inputField_SignNickname.isFocused)
        {
            inputField_SignId.Select();
        }
    }
}
