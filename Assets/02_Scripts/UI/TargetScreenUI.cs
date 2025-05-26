using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class TargetScreenUI : MonoBehaviour
{
    /*** 친구 UI 이동 구간 ***/
    /*** Record UI 이동 구간 ***/

    [SerializeField] private GameObject TargetScreen;
    private VideoPlayer video;
    public void GoToTargetScreen()
    {
        video = gameObject.GetComponentInChildren<VideoPlayer>();
        TargetScreen.SetActive(true);
    }

    public void BackToTargetScreen()
    {
        video.frame = 0;
        video.Play();
        TargetScreen.SetActive(false);
    }
}

