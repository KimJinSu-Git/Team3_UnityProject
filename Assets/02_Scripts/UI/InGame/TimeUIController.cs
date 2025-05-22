using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class TimeUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText;

    [Header("색상 설정")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color overtimeColor = Color.red;

    [Header("깜빡임 설정")]
    [SerializeField] private float blinkTime = 10f; 
    [SerializeField] private float blinkSpeed = 4f;

    private void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.currentState != GameState.Playing) return;

        float timeLeft = GameManager.Instance.GetTimeLeft();
        bool isOvertime = GameManager.Instance.IsInOvertime;

        timeText.text = FormatTime(timeLeft);

        if (isOvertime)
        {
            timeText.color = overtimeColor;
        }
        else
        {
            timeText.color = normalColor;
        }

        if (timeLeft <= blinkTime)
        {
            float alpha = Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed));
            Color c = timeText.color;
            c.a = alpha;
            timeText.color = c;
        }
    }

    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return $"{minutes:D2}:{secs:D2}";
    }
}
