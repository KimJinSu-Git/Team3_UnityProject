using UnityEngine;
using UnityEngine.UI;

public class MonsterHealthBar : MonoBehaviour
{ 
     private Slider slider;

    private void Awake()
    {
        if (slider == null)
            slider = GetComponent<Slider>();
    }

    public void SetMaxHealth(float maxHealth)
    {
        slider.maxValue = maxHealth;
        slider.value    = maxHealth;
    }

    public void SetHealth(float currentHealth)
    {
        slider.value = Mathf.Clamp(currentHealth, 0f, slider.maxValue);
    }
}