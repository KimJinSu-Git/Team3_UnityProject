using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetScreenUI : MonoBehaviour
{
    /*** 친구 UI 이동 구간 ***/
    /*** Record UI 이동 구간 ***/

    [SerializeField] private GameObject TargetScreen;
    public void GoToTargetScreen()
    {
        TargetScreen.SetActive(true);
    }

    public void BackToTargetScreen()
    {
        TargetScreen.SetActive(false);
    }
}
