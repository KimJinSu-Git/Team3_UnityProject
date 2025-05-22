using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElixirManager : MonoBehaviour
{
    public static ElixirManager Instance;

    [Header("엘릭서 수치")]
    [SerializeField] private float currentElixir = 5f;
    [SerializeField] private float maxElixir = 10f;
    
    public float GetCurrentElixir() => currentElixir;
    public float GetMaxElixir() => maxElixir;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!IsGameActive()) return;

        
        float regenRate = GetElixirRegenRate();
        currentElixir += regenRate * Time.deltaTime;
        
        currentElixir = Mathf.Clamp(currentElixir, 0f, maxElixir);

        if (Input.GetKeyDown(KeyCode.E))
        {
            UseElixir(3f);
            Debug.Log("엘릭서 3 사용");
        }
    }

    private bool IsGameActive()
    {
        return GameManager.Instance != null &&
               GameManager.Instance.currentState == GameState.Playing;
    }

    private float GetElixirRegenRate()
    {
        float elapsedTime = GameManager.Instance.MatchElapsedTime;

        if (elapsedTime < 120f)                 
            return 0.357f;                      
        else if (elapsedTime < 180f)            
            return 0.714f;                      
        else if (elapsedTime < 240f && GameManager.Instance.IsInOvertime)
            return 1.07f;                       
        else
            return 0f;                         
    }

    public bool UseElixir(float amount)
    {
        if (currentElixir >= amount)
        {
            currentElixir -= amount;

            return true;
        }

        return false;
    }
}