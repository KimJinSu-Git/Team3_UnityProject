using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerHealthBar : MonoBehaviour
{
    [Header("Slider & Fill")]
    public Slider      slider;       // 에디터에서 연결// Slider > Fill Area > Fill 의 Image

    [Header("HP Text (TMPro)")]
    public TMP_Text    hpText;       // 체력 숫자 표시용 TMP 텍스트

    private void Awake()
    {
        if (slider == null)
            slider = GetComponent<Slider>();
        
    }

    /// <summary>
    /// 최대 체력 설정 및 텍스트 초기화
    /// </summary>
    public void SetMaxHealth(float maxHealth)
    {
        slider.maxValue = maxHealth;
        slider.value    = maxHealth;

        if (hpText != null)
            hpText.text = maxHealth.ToString();
        
    }

    /// <summary>
    /// 체력 갱신(슬라이더·텍스트·Fill on/off)
    /// </summary>
    public void SetHealth(float currentHealth)
    {
        float clamped = Mathf.Clamp(currentHealth, 0f, slider.maxValue);
        slider.value = clamped;

        if (hpText != null)
            hpText.text = clamped.ToString();
        
    }
}
