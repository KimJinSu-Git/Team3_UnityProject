using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 최대치 10
// 1초당 1씩 회복

public class ElixirManager : MonoBehaviour
{
    public static ElixirManager Instance;

    [Header("엘릭서 수치")]
    [SerializeField] private float currentElixir = 5f; // 현재 엘릭서
    [SerializeField] private float maxElixir = 10f; // 최대 엘릭서
    [SerializeField] private float regenRate = 1f; // 엘릭서 회복량
    
    private float regenTimer = 0f;
    
    public float GetCurrentElixir()
    {
        return currentElixir;
    }

    public float GetMaxElixir()
    {
        return maxElixir;
    }
    
    private void Awake()
    {
        Instance = this;
    }
    
    private void Update()
    {
        if (!IsGameActive()) return;

        regenTimer += Time.deltaTime;
        if (regenTimer >= 1f)
        {
            AddElixir(regenRate);
            regenTimer = 0f;
        }

        // UI 업데이트 추가
        
    }
    
    private bool IsGameActive()
    {
        // 게임 상태가 Playing일 때만 엘릭서가 회복되기.
        return GameManager.Instance != null && GameManager.Instance.currentState == GameState.Playing;
    }

    public void AddElixir(float amount)
    {
        currentElixir += amount;
        currentElixir = Mathf.Clamp(currentElixir, 0f, maxElixir);
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
