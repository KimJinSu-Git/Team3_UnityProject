using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;

public class MainButtonUI : MonoBehaviour
{
    #region Matching UI 이동 구간
    [SerializeField] private GameObject[] matchingUI;
    [SerializeField] private Material transparentUI;
    [SerializeField] private RectTransform[] position;
    [SerializeField] private VideoPlayer videoPlayer;
    
    private bool isMatching = false;

    public void OnMatchingButtonClicked()
    {
        if (!isMatching)
        {
            StartMatching();
        }
        else
        {
            CancelMatching();
        }
        isMatching = !isMatching;
    }
    
    private void StartMatching() 
    {
        List<Transform> allChildren = new List<Transform>(GetComponentsInChildren<Transform>());

        foreach (Transform child in allChildren)
        {
            // 자기 자신 제외
            if (child.Equals(this.transform)) continue;

            // matchingUI 배열에 포함되어 있는지 확인
            bool isExcluded = false;
            foreach (GameObject match in matchingUI)
            {
                if (child.gameObject.Equals(match) || child.IsChildOf(match.transform)) 
                {
                    isExcluded = true;
                    break;
                }
            }

            if (isExcluded)
            {

                matchingUI[0].transform.DOScale(0.8f, 2f);
                Image image = matchingUI[1].GetComponentInChildren<Image>();
                image.DOColor(Color.red, 1f);
                
                TMP_Text text = matchingUI[1].GetComponentInChildren<TMPro.TMP_Text>();
                text.text = "매칭 중...";
                
                matchingUI[2].transform.DOMove(position[1].position, 1f);
                matchingUI[3].transform.DOMove(position[3].position, 1f);
            }
            else
            {
                // CanvasGroup이 없으면 추가
                CanvasGroup cg = child.GetComponent<CanvasGroup>();
                if (cg == null)
                    cg = child.gameObject.AddComponent<CanvasGroup>();

                // DOTween으로 천천히 사라지게 (예: 1초 동안)
                cg.DOFade(0f, 1f);
                transparentUI.DOFloat(0f, "_Alpha", 1f);
                videoPlayer.Stop();
            }
        }
    }

    private void CancelMatching()
    {
        List<Transform> allChildren = new List<Transform>(GetComponentsInChildren<Transform>());

        foreach (Transform child in allChildren)
        {
            if (child == this.transform) continue;

            bool isExcluded = false;
            foreach (GameObject match in matchingUI)
            {
                if (child.gameObject == match || child.IsChildOf(match.transform))
                {
                    isExcluded = true;
                    break;
                }
            }

            if (isExcluded)
            {
                matchingUI[0].transform.DOScale(0.7f, 1f);  // 원래 크기로 복구

                Image image = matchingUI[1].GetComponentInChildren<Image>();
                if (image != null)
                    image.DOColor(Color.yellow, 1f);  // 원래 색깔로 복구

                TMP_Text text = matchingUI[1].GetComponentInChildren<TMP_Text>();
                if (text != null)
                    text.text = "전투";
                
                matchingUI[2].transform.DOMove(position[0].position, 1f);
                matchingUI[3].transform.DOMove(position[2].position, 1f);
            }
            else
            {
                CanvasGroup cg = child.GetComponent<CanvasGroup>();
                if (cg == null)
                    cg = child.gameObject.AddComponent<CanvasGroup>();

                cg.DOFade(1f, 1f);  // 다시 보이게 복구
                transparentUI.DOFloat(1f, "_Alpha", 1f);
                videoPlayer.Play();
            }
        }
    }

    #endregion
    
}
