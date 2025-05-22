using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CrownScoreController : MonoBehaviour
{
    [Header("왕관 프리팹")]
    [SerializeField] private GameObject crownFlyPlayerPrefab; // 아군용 왕관
    [SerializeField] private GameObject crownFlyEnemyPrefab;  // 적군용 왕관

    [Header("목표 위치 및 UI")]
    [SerializeField] private RectTransform scoreTextTarget;     // 점수 텍스트 위치 받아오기
    [SerializeField] private TextMeshProUGUI crownScoreText;    // 변할 왕관 점수 텍스트
    [SerializeField] private Canvas canvas;

    private int currentScore = 0;

    public void AddCrownsFromPositions(List<Vector3> worldPositions, bool isPlayerSide)
    {
        StartCoroutine(AnimateCrowns(worldPositions, isPlayerSide));
    }

    private IEnumerator AnimateCrowns(List<Vector3> worldPositions, bool isPlayerSide)
    {
        foreach (Vector3 worldPos in worldPositions)
        {
            SpawnCrownAndFly(worldPos, isPlayerSide);
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void SpawnCrownAndFly(Vector3 worldPos, bool isPlayerSide)
    {
        GameObject prefab = isPlayerSide ? crownFlyPlayerPrefab : crownFlyEnemyPrefab;
        GameObject crown = Instantiate(prefab, canvas.transform);
        RectTransform crownRT = crown.GetComponent<RectTransform>();

        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), screenPos, null, out Vector2 uiStartPos);
        crownRT.anchoredPosition = uiStartPos;

        StartCoroutine(FlyToTargetAndAddScore(crownRT, scoreTextTarget.anchoredPosition, 0.5f));
    }

    private IEnumerator FlyToTargetAndAddScore(RectTransform crownRT, Vector2 targetPos, float duration)
    {
        float t = 0f;
        Vector2 startPos = crownRT.anchoredPosition;

        while (t < duration)
        {
            t += Time.deltaTime;
            crownRT.anchoredPosition = Vector2.Lerp(startPos, targetPos, t / duration);
            yield return null;
        }

        Destroy(crownRT.gameObject);
        currentScore++;
        if (currentScore >= 3)
        {
            currentScore = 3;
        }
        crownScoreText.text = currentScore.ToString();
    }
}
