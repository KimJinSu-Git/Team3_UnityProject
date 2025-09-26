using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchingButton : MonoBehaviour
{
    private Button button;
    void Start()
    {
        TryGetComponent(out button);
        button.onClick.AddListener(OnMatchingButton);
    }
    void OnMatchingButton()
    {
        SessionManager.Instance.StartMatchMaking();
    }
}
