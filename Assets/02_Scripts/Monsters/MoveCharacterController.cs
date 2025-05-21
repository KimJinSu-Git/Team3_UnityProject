using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class MoveCharacterController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    [SerializeField] MonsterData monsterData;
    
    public enum CharacterState {Idle, Attack, Walk}
    [FormerlySerializedAs("currentstate")] public CharacterState currentState = CharacterState.Idle;
    public CharacterState prevState;
    
    private bool isStunned = false;
    private Transform[] targetPosition;
    private bool walk;
    private int brokenCastle = 0;
    
    private float spawnTimer = 0f;
    private bool isSpawnWaiting = true;

    private void Start()
    {
        walk = false;
        TryGetComponent(out agent);
        TryGetComponent(out animator);
        // 부모에서 DestinationSetting 컴포넌트를 찾아 targetPosition 가져오기
        DestinationSetting destinationSetting = GetComponentInParent<DestinationSetting>();
        targetPosition = destinationSetting.targetPosition;
        
        BrokenCastleManager.OnBrokenCastle += SetDestination;
        //초기 위치 설정
        SetDestination(brokenCastle);
    }

    private void Update()
    {
        if (isStunned)
        {
            return;
        }

        if (isSpawnWaiting)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer > monsterData.spawnTime)
            {
                ChangeState(CharacterState.Walk);
                isSpawnWaiting = false;
            }
        }
        switch (currentState)
        {
            case CharacterState.Idle:
                PerformIdle();
                break;
            case CharacterState.Walk :
                PerformWalk();
                break;
            case CharacterState.Attack:
                PerformAttack();
                break;
        }

    }

    private void PerformIdle()
    {
        
    }
    
    private void PerformAttack()
    {
        
    }

    private void PerformWalk()
    {
        animator.SetBool("Walk", true);
        switch (brokenCastle)
        {
            case 0:
                MoveToClosest(0, 1, 2);
                break;
            case 1:
                MoveToClosest(1, 2);
                break;
            case 2:
                MoveToClosest(0, 2);
                break;
            case 3:
                agent.isStopped = true; // 이동 중단
                break;
        }
    }
    
    private void ChangeState(CharacterState newState)
    {
        prevState = currentState;
        currentState = newState;
    }
    
    private void MoveToClosest(int indexA, int indexB, int indexC = -1)
    {
        float distA = Vector3.Distance(transform.position, targetPosition[indexA].position);
        float distB = Vector3.Distance(transform.position, targetPosition[indexB].position);
        float distC = indexC >= 0 ? Vector3.Distance(transform.position, targetPosition[indexC].position) : float.MaxValue;

        int closestIndex = indexA;
        float minDist = distA;

        if (distB < minDist)
        {
            closestIndex = indexB;
            minDist = distB;
        }

        if (indexC >= 0 && distC < minDist)
        {
            closestIndex = indexC;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(targetPosition[closestIndex].position);
        }
    }

    private void SetDestination(int castleState)
    {
        brokenCastle = castleState;

        if (brokenCastle == 3)
        {
            agent.isStopped = true;
        }
    }

    private void OnDestroy()
    {
        BrokenCastleManager.OnBrokenCastle -= SetDestination;
    }
}
