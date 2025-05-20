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
    public float matchTime = 180f; // 3분
    private float timeLeft;
    
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
        // UIManager.Instance.UpdateTimerUI(timeLeft);

        if (timeLeft <= 0)
        {
            Debug.Log("시간 종료!");
            EndGame();
        }
    }
    
    private void PrepareGame()
    {
        Debug.Log("게임 준비 완료, 3초 뒤 시작");
        currentState = GameState.Ready;

        // 3초 후 게임 시작
        Invoke(nameof(StartGame), 3f);
    }

    private void StartGame()
    {
        Debug.Log("게임 시작!");
        currentState = GameState.Playing;
        timeLeft = matchTime;
    }

    public void OnKingTowerDestroyed(TowerController tower)
    {
        if (currentState != GameState.Playing) return;
        
        Debug.Log($"킹 타워 파괴됨! 즉시 게임 종료");

        if (IsEnemyTower(tower))
        {
            myCrowns += 1;
        }
        else
        {
            enemyCrowns += 1;
        }

        EndGame();
    }

    public void OnPrincessTowerDestroyed(TowerController tower)
    {
        if (currentState != GameState.Playing) return;
        
        Debug.Log($"{tower.towerType} 파괴됐어용 -> 왕관+1 ");
        
        // 부셔진 게 내 타워인지 상대 타워인지 판단
        if(IsEnemyTower(tower))
        {
            myCrowns++;
        }
        else
        {
            enemyCrowns++;
        }
    }

    private void EndGame()
    {
        if (currentState == GameState.Ended) return;

        currentState = GameState.Ended;
        Debug.Log("게임 종료");

        string result;
        
        if (myCrowns > enemyCrowns)
            result = "승리!";
        else if (myCrowns < enemyCrowns)
            result = "패배!";
        else
            result = "무승부!";

        Debug.Log($"게임 종료. 결과: {result}");

        // 결과창 UI 호출 민규 씨 UI 연동 예시
        // UIManager.Instance.ShowResult(result, myCrowns, enemyCrowns);
        
        // 씬 이동 or 대기 처리 등 기능 추가 가능
    }

    private bool IsEnemyTower(TowerController tower)
    {
        return tower.CompareTag("EnemyTower");
    }
}
