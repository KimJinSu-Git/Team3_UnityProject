using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchingButton : MonoBehaviour
{
    public Button matchingButton;
    void Start()
    {
        TryGetComponent(out matchingButton);
        matchingButton.onClick.AddListener(OnMyButtonClick);
    }

    void OnMyButtonClick()
    {
        SessionManager.Instance.StartMatchMaking();
    }
}
