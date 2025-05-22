using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Ready,
    Playing,
    Ended
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("왕관 수")]
    private int myCrowns = 0;
    private int enemyCrowns = 0;

    [Header("게임 상태")]
    public GameState currentState = GameState.Ready;

    [Header("게임 타이머")]
    private float timeLeft;
    private float gameStartTime;
    private const float originalMatchDuration = 180f; 
    public float MatchElapsedTime => Time.time - gameStartTime;

    public bool IsInOvertime { get; private set; } = false;
    
    public float GetTimeLeft()
    {
        return timeLeft;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        PrepareGame();
    }

    private void Update()
    {
        if (currentState != GameState.Playing) return;
        
        timeLeft -= Time.deltaTime;

        float elapsed = MatchElapsedTime;

        // 🕒 서든데스 진입 조건: 3분 경과 && 왕관 동점
        if (!IsInOvertime && elapsed >= 180f && AreCrownsTied())
        {
            Debug.Log("서든데스 시작!");
            IsInOvertime = true;
            timeLeft = 60f; // 1분 추가
            return; // 다음 프레임부터 종료 검사
        }

        if (timeLeft <= 0)
        {
            Debug.Log("경기 종료!");
            EndGame();
        }
    }

    private void PrepareGame()
    {
        Debug.Log("게임 준비 완료, 3초 뒤 시작");
        currentState = GameState.Ready;
        Invoke(nameof(StartGame), 3f);
    }

    private void StartGame()
    {
        Debug.Log("게임 시작!");
        currentState = GameState.Playing;
        timeLeft = originalMatchDuration;
        IsInOvertime = false;
        gameStartTime = Time.time;
    }

    public void OnPrincessTowerDestroyed(TowerController tower)
    {
        if (currentState != GameState.Playing) return;

        Debug.Log($"{tower.towerType} 파괴 → 왕관 +1");

        if (IsEnemyTower(tower)) myCrowns++;
        else enemyCrowns++;
    }

    public void OnKingTowerDestroyed(TowerController tower)
    {
        if (currentState != GameState.Playing) return;

        Debug.Log("킹 타워 파괴 → 즉시 종료");

        if (IsEnemyTower(tower)) myCrowns = 3;
        else enemyCrowns = 3;

        EndGame();
    }

    private void EndGame()
    {
        if (currentState == GameState.Ended) return;

        currentState = GameState.Ended;

        string result;
        if (myCrowns > enemyCrowns) result = "승리!";
        else if (myCrowns < enemyCrowns) result = "패배!";
        else result = "무승부!";

        Debug.Log($"게임 종료. 결과: {result}");

        // 민규 씨 UI 연동 예시
        // UIManager.Instance.ShowResult(result, myCrowns, enemyCrowns);
    }

    private bool IsEnemyTower(TowerController tower)
    {
        return tower.CompareTag("EnemyTower");
    }

    public bool AreCrownsTied()
    {
        return myCrowns == enemyCrowns;
    }
}