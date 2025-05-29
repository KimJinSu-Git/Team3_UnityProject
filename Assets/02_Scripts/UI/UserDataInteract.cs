using TMPro; // ✅ 꼭 포함
using UnityEngine;

public class UserDataInteract : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI userName; // ✅ UI라면 TextMeshProUGUI 사용

    private void Start()
    {
        if (UserManager.Instance != null && UserManager.Instance.userData != null)
        {
            userName.text = UserManager.Instance.userData.DisplayName;
        }
        else
        {
            userName.text = "유저 정보 없음";
        }
    }
}