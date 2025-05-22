using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TabSlider : MonoBehaviour
{
    public RectTransform contentPanel;
    public float slideDuration = 0.3f;
    public float panelWidth = 720f;

    public void MoveToTab(int index)
    {
        float targetX = -panelWidth * index;
        contentPanel.DOAnchorPosX(targetX, slideDuration).SetEase(Ease.OutCubic);
    }
}