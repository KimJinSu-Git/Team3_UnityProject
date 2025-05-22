using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElixirBarUIController : MonoBehaviour
{
    [System.Serializable]
    public class ElixirSlot
    {
        public Image slotBackground;
        public Image slotFill; 
    }
    
    [Header("엘릭서 슬롯들")]
    public ElixirSlot[] slots = new ElixirSlot[10];
    
    [Header("엘릭서 숫자 표시")]
    [SerializeField] private TextMeshProUGUI elixirCountText;

    private void Update()
    {
        if (ElixirManager.Instance == null) return;

        float elixir = ElixirManager.Instance.GetCurrentElixir();

        for (int i = 0; i < slots.Length; i++)
        {
            float fill = Mathf.Clamp01(elixir - i); // 각 칸의 fill 비율
            slots[i].slotFill.fillAmount = fill;

            if (fill >= 1f)
            {
                slots[i].slotFill.color = new Color(1f, 0.6f, 1f, 1f); 
            }
            else
            {
                slots[i].slotFill.color = new Color(1f, 1f, 1f, 0.3f); 
            }
        }
        
        elixirCountText.text = Mathf.FloorToInt(elixir).ToString();
    }
}
