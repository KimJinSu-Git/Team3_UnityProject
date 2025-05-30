using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEditor.Compilation;
using UnityEngine;

public enum GameState
{
    Ready,
    Playing,
    Ended
}

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    public bool ended;
    //[Header("왕관 수")]
    [Networked] private int myCrowns {get; set;}
    [Networked] private int enemyCrowns {get; set;}
    public int MyCrowns => myCrowns;
    public int EnemyCrowns => enemyCrowns;
    
    [Header("게임 상태")]
    public GameState currentState = GameState.Ready;

    [Header("게임 타이머")]
    private float timeLeft;
    private float gameStartTime;
    private const float originalMatchDuration = 180f;
    public float MatchElapsedTime => Time.time - gameStartTime;

    public bool IsInOvertime { get; private set; } = false;
    public float GetTimeLeft() => timeLeft;
    

    [Header("왕관 UI 컨트롤러")]
    public CrownScoreController playerCrownUI;
    public CrownScoreController enemyCrownUI;

    [Header("타워 리스트")]
    public List<TowerController> playerPrincessTowers;
    public List<TowerController> enemyPrincessTowers;

    public Transform Area;
    private void Awake()
    {
        Instance = this;
        ended = false;
    }

    private void Start() => PrepareGame();

    private void Update()
    {
        if (currentState != GameState.Playing) return;

        timeLeft -= Time.deltaTime;

        float elapsed = MatchElapsedTime;
        if (!IsInOvertime && elapsed >= 180f && AreCrownsTied())
        {
            Debug.Log("서든데스 시작!");
            IsInOvertime = true;
            timeLeft = 60f;
            return;
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
        if (UserManager.Instance.FusionPlayerRef == SessionManager.Instance.CurrentGameRoomInfo.ClientPlayer)
        {
            // Vector3 rotation = Area.transform.rotation.eulerAngles;
            // rotation.y = 180;
            // 카메라 세팅
            Camera.main.transform.position = new Vector3(0, 9, 3);
            Camera.main.transform.rotation = Quaternion.Euler(70, 180, 0);
        }
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

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_OnPrincessTowerDestroyed(TowerController tower)
    {
        if (currentState != GameState.Playing ) return;

        Debug.Log($"{tower.towerType} 파괴 → 왕관 +1");

        if (IsEnemyTower(tower))
        {
            //myCrowns++;
            RPC_CrownUp(true);
            playerCrownUI.AddCrownsFromPositions(new List<Vector3> { tower.transform.position }, true);
        }
        else
        {
            //enemyCrowns++;
            RPC_CrownUp(false);
            enemyCrownUI.AddCrownsFromPositions(new List<Vector3> { tower.transform.position }, false);
        }
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    private void RPC_CrownUp(bool playerType)
    {
        if (playerType)
        {
            myCrowns++;
        }
        else
        {
            enemyCrowns++;
        }
    }
    
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_OnKingTowerDestroyed(TowerController tower)
    {
        if (currentState != GameState.Playing) return;

        Debug.Log("킹 타워 파괴 → 즉시 종료");

        List<Vector3> crownSpawnPositions = new List<Vector3>();
        crownSpawnPositions.Add(tower.transform.position);

        List<TowerController> targetPrincessTowers = IsEnemyTower(tower) ? enemyPrincessTowers : playerPrincessTowers;
        foreach (var pricessTower in targetPrincessTowers)
        {
            if (pricessTower != null && pricessTower.IsAlive)
            {
                pricessTower.ForceDestroy();
                crownSpawnPositions.Add(pricessTower.transform.position);
            }
        }

        if (IsEnemyTower(tower))
        {
            myCrowns = 3;
            playerCrownUI.AddCrownsFromPositions(crownSpawnPositions, true);
        }
        else
        {
            enemyCrowns = 3;
            enemyCrownUI.AddCrownsFromPositions(crownSpawnPositions, false);
        }

        
    }

    public void EndGame() //Host에서만 게임 종료
    {
        if (currentState == GameState.Ended) return;

        currentState = GameState.Ended;
        ended = true;

        string result = (myCrowns > enemyCrowns) ? "승리!" : (myCrowns < enemyCrowns ? "패배!" : "무승부!");

        Debug.Log($"게임 종료. 결과: {result}");

        if (Object.HasStateAuthority)
        {
            RPC_NotifyGameEnded();
        }
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    private void RPC_NotifyGameEnded() // 클라이언트 게임 종료
    {
        currentState = GameState.Ended;
        ended = true;
    }

    private bool IsEnemyTower(TowerController tower) => tower.CompareTag("EnemyTower");
    public bool AreCrownsTied() => myCrowns == enemyCrowns;
}