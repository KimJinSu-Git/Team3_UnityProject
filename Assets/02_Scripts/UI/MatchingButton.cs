using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchingButton : MonoBehaviour
{
    public Button matchingButton;
    public SessionManager sessionManager;
    void Start()
    {
        TryGetComponent(out matchingButton);
        matchingButton.onClick.AddListener(OnMyButtonClick);
    }

    void OnMyButtonClick()
    {
        sessionManager.StartMatchMaking();
    }
}
