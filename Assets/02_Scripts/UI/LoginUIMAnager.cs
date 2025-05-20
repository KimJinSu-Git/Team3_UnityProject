using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;

public class LoginUIManager : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text errorText;

    private FirebaseAuth auth;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        errorText.gameObject.SetActive(false); // 시작 시 오류 메시지 숨기기
    }

    public void OnClickLogin()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowError("이메일과 비밀번호를 입력하세요.");
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                ShowError("로그인 실패: 정보를 다시 확인하세요.");
            }
            else
            {
                FirebaseUser user = task.Result.User;
                Debug.Log("로그인 성공: " + user.Email);
                HideError();
                // 씬 전환 또는 Fusion 연결
            }
        });
    }

    public void OnClickRegister()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowError("이메일과 비밀번호를 입력하세요.");
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                ShowError("회원가입 실패: 이미 존재하는 이메일일 수 있습니다.");
            }
            else
            {
                FirebaseUser newUser = task.Result.User;
                Debug.Log("회원가입 성공: " + newUser.Email);
                HideError();
                // Firestore에 정보 저장 가능
            }
        });
    }

    public void OnClickGuestLogin()
    {
        auth.SignInAnonymouslyAsync().ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                ShowError("게스트 로그인 실패");
            }
            else
            {
                FirebaseUser guest = task.Result.User;
                Debug.Log("게스트 로그인 성공: " + guest.UserId);
                HideError();
                // 씬 전환 또는 Fusion 연결
            }
        });
    }

    private void ShowError(string message)
    {
        errorText.text = message;
        errorText.color = Color.red;
        errorText.gameObject.SetActive(true);
    }

    private void HideError()
    {
        errorText.text = "";
        errorText.gameObject.SetActive(false);
    }
}
