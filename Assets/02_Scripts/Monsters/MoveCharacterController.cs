using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Vector3 = UnityEngine.Vector3;

public class MoveCharacterController : MonoBehaviour
{
    private static readonly int Walk = Animator.StringToHash("Walk");
    
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
        
        // 부모에서 DestinationSetting 컴포넌트를 찾아 targetPosition 가져오기 && LayerMask바꾸기
        DestinationSetting destinationSetting = GetComponentInParent<DestinationSetting>();
        targetPosition = destinationSetting.targetPosition;
        string layerName = destinationSetting.LayerDes.ToString();
        gameObject.layer = LayerMask.NameToLayer(layerName);
        
        agent.baseOffset = 0;
        agent.speed = monsterData.moveSpeed;
        agent.acceleration = 999;
        agent.autoBraking = false;
        
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
                if (agent.velocity.magnitude > 0 && animator.GetBool(Walk).Equals(false))
                {
                    animator.SetBool(Walk, true);
                }
                PerformWalk();
                break;
            case CharacterState.Attack:
                PerformAttack();
                break;
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position + Vector3.up * 1f, monsterData.attackRange);
        
        
    }

    private void PerformIdle()
    {
        float distanceToTarget = Vector3.Distance(transform.position, agent.destination);

        // 일정 거리 이상 떨어지면 다시 Walk 상태로 전환
        if (distanceToTarget > agent.stoppingDistance + 0.1f) // 여유 거리
        {
            ChangeState(CharacterState.Walk);
        }
    }

    
    private void PerformAttack()
    {
    }

    private void PerformWalk()
    {

        switch (brokenCastle)
        {
            case 0:
                MoveToClosest(true, true);
                break;
            case 1:
                MoveToClosest(false, true);
                break;
            case 2:
                MoveToClosest(true, false);
                break;
            case 3:
                MoveToClosest(false, false);
                break;
            case 4:
                agent.isStopped = true;
                break;
        }
    }
    
    private void ChangeState(CharacterState newState)
    {
        prevState = currentState;
        currentState = newState;
    }
    
    private void MoveToClosest(bool rightBuild = false, bool leftBuild = false)
    {
        float distA = Vector3.Distance(transform.position, targetPosition[0].position); // 으론쪽
        float distB = Vector3.Distance(transform.position, targetPosition[1].position); // 왼쪽
        float distC = Vector3.Distance(transform.position, targetPosition[2].position); // 왕

        int targetIndex = 2; // 기본은 King

        if (rightBuild && leftBuild)
        {
            // 세 개 다 비교
            if (distA <= distB && distA <= distC)
                targetIndex = 0; // 오른쪽
            else if (distB <= distA && distB <= distC)
                targetIndex = 1; // 왼쪽
            else
                targetIndex = 2; // 킹
        }
        else if (rightBuild && leftBuild.Equals(false))
        {
            targetIndex = (distA < distC) ? 0 : 2;
        }
        else if (rightBuild.Equals(false) && leftBuild)
        {
            targetIndex = (distB < distC) ? 1 : 2;
        }
        else
        {
            targetIndex = 2; // 둘 다 파괴됐으면 King
        }

        // 이동 명령
        if (!agent.pathPending)
        {
            agent.isStopped = false;
            agent.SetDestination(targetPosition[targetIndex].position);
            animator.SetBool(Walk, true); // 이 시점에서 바로 걷기 애니메이션 ON
        }

        // 목적지에 도착했는지 체크
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.isStopped = true;
            animator.SetBool(Walk, false);
            ChangeState(CharacterState.Idle);
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
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 1f, monsterData.attackRange);
    }
    

    private void OnDestroy()
    {
        BrokenCastleManager.OnBrokenCastle -= SetDestination;
    }
}
