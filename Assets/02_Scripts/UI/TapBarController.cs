using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class TabBarController : MonoBehaviour
{
    public List<Button> tabButtons;
    public RectTransform highlightBar;
    public float highlightMoveDuration = 0.3f;
    public float selectedScale = 1.2f;
    public float defaultScale = 1f;

    private Button currentSelected;

    public void OnTabSelected(Button selectedButton)
    {
        if (currentSelected != null)
            currentSelected.transform.DOScale(defaultScale, 0.2f);

        selectedButton.transform.DOScale(selectedScale, 0.2f).SetEase(Ease.OutBack);
        currentSelected = selectedButton;

        // Highlight 바 이동
        Vector2 targetPos = selectedButton.GetComponent<RectTransform>().anchoredPosition;
        highlightBar.DOAnchorPosX(targetPos.x, highlightMoveDuration).SetEase(Ease.OutCubic);
    }
    
}