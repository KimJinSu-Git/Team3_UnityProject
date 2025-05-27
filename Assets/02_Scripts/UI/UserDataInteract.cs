using TMPro;
using UnityEngine;

public class UserDataInteract : MonoBehaviour
{
    [SerializeField] private TextMeshPro userName;

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