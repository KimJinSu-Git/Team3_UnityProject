using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    public Slider slider;  // 에디터에서 연결
    public TMP_Text healthText;
    public void SetMaxHealth(float maxHealth)
    {
        slider.maxValue = maxHealth;
        slider.value    = maxHealth;
        healthText.text = maxHealth.ToString();
    }
    
    public void SetHealth(float currentHealth)
    {
        float clamped = Mathf.Clamp(currentHealth, 0f, slider.maxValue);
        slider.value = clamped;
        healthText.text = clamped.ToString();
    }
}