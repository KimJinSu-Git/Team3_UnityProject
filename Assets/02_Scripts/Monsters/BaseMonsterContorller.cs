using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Fusion;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Vector3 = UnityEngine.Vector3;

public class BaseMonsterContorller : NetworkBehaviour, IDamageAble
{
    //IDamgeable 구성요소
    public GameObject GameObject => gameObject;
    public Collider Collider => mainCollider;
    public PlayerRef PlayerRef => playerRef;
    public NetworkObject NetworkObject => networkObject;
    
    public PlayerRef playerRef;
    private NetworkObject networkObject;


    public bool isDie;

    private Collider mainCollider;
    
    public enum CharacterState {Idle, Attack, Walk}
    private static readonly int Walk = Animator.StringToHash("Walk");
    
    [Header("Player내부 Component")]
    private NavMeshAgent agent;
    private Animator animator;
    [SerializeField] MonsterData monsterData;
    private Collider collider;
    
    [SerializeField] private Stat MonsterStat = new Stat();
    
    [Header("감지 대상")]
    [SerializeField] private HashSet<Transform> targetPosition = new HashSet<Transform>();
    //[SerializeField] private List<Transform> targetPosition = new List<Transform>(); 잘 들어가는지 보고 싶다면
    //위에꺼 주석하고 이거하면 뭐 들어가는지 볼 수 있음

    
    [Header("캐릭터 이전과 현재 상태")]
    public CharacterState currentState = CharacterState.Idle;
    public CharacterState prevState;
    
    [Header("스턴")]
    private bool isStunned = false;
    [Header("이동로직")]
    private bool walk;
    private static int brokenCastle;
    public bool hasObstacle;
    
    [Header("처음 스폰했을 때 대기시간으로 사용되는 것")]
    private float spawnTimer = 0f;
    private bool isSpawnWaiting = true;
    
    private void Start()
    {
        networkObject = Object;
        walk = false;
        TryGetComponent(out agent);
        TryGetComponent(out animator);
        TryGetComponent(out collider);
        TryGetComponent(out mainCollider);
        
        agent.baseOffset = 0;
        agent.speed = monsterData.moveSpeed;
        agent.acceleration = 999;
        agent.autoBraking = false;
        
        BrokenCastleManager.OnBrokenCastle += SetDestination;
        
        //체력 설정 
        MonsterStat.maxHP = monsterData.maxHP;
        MonsterStat.level = monsterData.level;
        MonsterStat.currentHp = (float)monsterData.maxHP;
        MonsterStat.Damage = monsterData.damage;
        
        //초기 위치 설정
        SetDestination(brokenCastle);
    }
    // 네트워크 데미지 처리
    public void TakeDamage(int damage)
    {
        if (isDie) return;

        MonsterStat.currentHp -= damage;

        if (MonsterStat.currentHp <= 0)
        {
            isDie = true;
            RPC_OnDie();
        }
    }
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public virtual void RPC_OnDie()
    {
        if (Object.HasStateAuthority == false) return;
        Runner.Despawn(Object);
    }
    //////////////////////////////////////////////////
    private void Update()
    {
        //상속 받고 나서 이동목표물을 Setting 메소드를 TowerDestinationSet인지 MonsterTowerDestinationSet 구분해줄 것.
        //MonsterTowerDestinationSet();
        TowerDestinationSet();
        //상속 받고 나서 공격목표물 Setting 메소드를 TowerTarget인지 TowerMonsterTarget인지 구분해줄 것.
        TowerTargetSetting();
        //0MonsterTowerTargetSetting();
        
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
    }

    private void PerformIdle()
    {
        float distanceToTarget = Vector3.Distance(transform.position, agent.destination);

        // 일정 거리 이상 떨어지면 다시 Walk 상태로 전환
        if (distanceToTarget > agent.stoppingDistance + 0.1f) // 여유 거리
        {
            ChangeState(CharacterState.Walk);
        }
        else
        {
            ChangeState(CharacterState.Attack);
        }
    }

    private bool isAttacking = false;
    private float attackTimer;
    
    private void PerformAttack()
    {
        Debug.Log("공격합니다.");
        if (!isAttacking)
        {
            isAttacking = true;
            attackTimer = 0f;  
            // 공격 애니메이션 실행
            animator.SetTrigger("Attack");

            // TODO: 여기서 공격 판정 로직도 넣을 수 있음 (예: target HP 감소 등)
            if (currentAttackTarget != null)
            {
                if (currentAttackTarget.TryGetComponent(out IDamageAble damageTarget))
                {
                    damageTarget.TakeDamage(monsterData.damage);
                }
            }
        }

        // 공격이 일정 시간 지나면 종료
        attackTimer += Time.deltaTime;
        if (attackTimer > 3f/*monsterData.attackSpeed*/) // 애니메이션 길이에 맞게 조절
        {
            isAttacking = false;
            ChangeState(CharacterState.Idle);
        }
    }
    private Transform currentAttackTarget; // 클래스 변수로 선언

    private void PerformWalk()
    {
        if (targetPosition.Count.Equals(0)) return;

        float minDistance = float.MaxValue;
        Transform closestTarget = null;

        // 파괴된 대상은 미리 제거
        var toRemove = new List<Transform>();
    
        foreach (var target in targetPosition)
        {
            if (target == null)
            {
                toRemove.Add(target); //null이면 없는거니까
                continue;
            }

            float distance = Vector3.Distance(transform.position, target.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestTarget = target;
            }
        }

        // 파괴된 Transform 정리
        foreach (var remove in toRemove)
        {
            targetPosition.Remove(remove);
        }
        currentAttackTarget = closestTarget; // 여기서 설정
        Vector3 targetPos = closestTarget.position;
        
        float distanceToTarget = Vector3.Distance(transform.position, targetPos);
        // 1. 직선 경로에 장애물이 있는지 Raycast로 확인
        // 장애물 존재 여부 판단
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.2f;
        Vector3 direction = (targetPos - transform.position).normalized;
        hasObstacle = false;

        if (Physics.Raycast(origin, direction, out hit,
                distanceToTarget, LayerMask.GetMask("Obstacle", "Tower")))
        {
            GameObject hitObj = hit.collider.gameObject;
            int hitLayer = hitObj.layer;
            
            if(hitLayer.Equals(LayerMask.NameToLayer("Obstacle")))
            {
                hasObstacle = true;
            }
            else if(hitLayer.Equals(LayerMask.NameToLayer("Tower")))
            {
                if (hitObj.TryGetComponent(out TowerController towerController))
                {
                    if (towerController.GetComponent<IDamageAble>().PlayerRef == UserManager.Instance.FusionPlayerRef)
                    {
                        hasObstacle = true; // 아군 타워면 장애물

                    }
                    // if (towerController.ownerType.Equals(this.ownerType))
                    // {
                    //     hasObstacle = true; // 아군 타워면 장애물
                    // }
                }
            }
        }
        
        
        if (hasObstacle)
        {
            agent.isStopped = false;
            agent.SetDestination(targetPos);
            animator.SetBool(Walk, true);
        }
        else
        { 
            NavMeshHit hit1;
            if (NavMesh.Raycast(transform.position, targetPos, out hit1, NavMesh.AllAreas))
            {
                SetAgentDestination(hit1.position);
            }
            else
            {
                if (NavMesh.SamplePosition(targetPos, out NavMeshHit sampleHit, 0.001f, NavMesh.AllAreas))
                {
                    SetAgentDestination(sampleHit.position);
                }
                else
                {
                    Debug.LogWarning("목표 지점이 NavMesh와 너무 멀리 떨어져 있어요!");
                }
            }
        }
    }
    
    public void TakeDamage(int combatEvent, bool OnDamage)
    {   
        MonsterStat.currentHp -= combatEvent;
        Debug.Log($"아파용 {monsterData.monsterName} : HP = {MonsterStat.currentHp}/{monsterData.maxHP}"); 
        
        if (MonsterStat.currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{monsterData.monsterName}이 죽었어요");
        Destroy(gameObject);
    }
    private void ChangeState(CharacterState newState)
    {
        prevState = currentState;
        currentState = newState;
    }

    private void SetAgentDestination(Vector3 destination)
    {
        if (!agent.pathPending)
        {
            agent.isStopped = false;
            agent.SetDestination(destination);
            animator.SetBool(Walk, true);
        }

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
        ChangeState(CharacterState.Walk);
        if (brokenCastle == 3)
        {
            agent.isStopped = true;
        }
    }

    private void TowerDestinationSet()
    {
        Collider[] TowerColliders = Physics.OverlapSphere(transform.position + Vector3.up * 0.1f,
            40f, 1 << LayerMask.NameToLayer("Tower"));

        foreach (Collider TowerCollider in TowerColliders)
        {
            if (TowerCollider.TryGetComponent(out TowerController towerController))
            {
                // 타워의 PlayerRef와 나의 PlayerRef를 비교
                if (towerController.GetComponent<IDamageAble>().PlayerRef != UserManager.Instance.FusionPlayerRef)
                {
                    targetPosition.Add(TowerCollider.transform);
                    Debug.Log($"targetPosition.Count = {targetPosition.Count}");
                }
            }
            else if (TowerCollider.TryGetComponent(out BaseMonsterContorller building))
            {
                if (building.GetComponent<IDamageAble>().PlayerRef != UserManager.Instance.FusionPlayerRef)
                {
                    targetPosition.Add(TowerCollider.transform);
                    Debug.Log($"targetPosition.Count = {targetPosition.Count}");
                }
            }
        }
    }

    private void TowerTargetSetting()
    {
        //단순히 타워때리는 애일 경우
        Collider[] towerColliders = Physics.OverlapSphere(transform.position + Vector3.up * 0.1f,
            monsterData.attackareaRange, 1 << LayerMask.NameToLayer("Tower"));
        
        foreach (var col in towerColliders)
        {
            if (col.GetComponent<IDamageAble>().PlayerRef != UserManager.Instance.FusionPlayerRef)
            {
                ChangeState(CharacterState.Attack);
                return;
            }
        }
    }
    private void MonsterTowerDestinationSet()
    {
        Collider[] TowerColliders = Physics.OverlapSphere(transform.position + Vector3.up * 0.1f,
            40f, 1 << LayerMask.NameToLayer("Tower"));
        Collider[] MonsterColliders = Physics.OverlapSphere(transform.position + Vector3.up * 0.1f,
            monsterData.attackareaRange + 5f, 1 << LayerMask.NameToLayer("Monster"));
        
        foreach (Collider TowerCollider in TowerColliders)
        {
            if (TowerCollider.GetComponent<IDamageAble>().PlayerRef != UserManager.Instance.FusionPlayerRef)
            {
                targetPosition.Add(TowerCollider.transform);
                Debug.Log($"targetPosition.Count = {targetPosition.Count}");
            }
        }

        foreach (Collider monsterCollider in MonsterColliders)
        {
            if (monsterCollider.GetComponent<IDamageAble>().PlayerRef != UserManager.Instance.FusionPlayerRef)
            {
                targetPosition.Add(monsterCollider.transform);
                Debug.Log($"targetPosition.Count = {targetPosition.Count}");

            }
        }
    }

    private void MonsterTowerTargetSetting()
    {
        // 주변의 Tower와 Monster 감지
        Collider[] MonsterTowerColliders = Physics.OverlapSphere(transform.position + Vector3.up * 0.1f,
            monsterData.attackareaRange);

        foreach (var col in MonsterTowerColliders)
        {
            if (col.GetComponent<IDamageAble>().PlayerRef != UserManager.Instance.FusionPlayerRef)
            {
                ChangeState(CharacterState.Attack);
                return;
            }
        }
    }

    protected void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.1f, 40f); //전체 탐색

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.1f, monsterData.attackareaRange);  //영역 탐색
        
    }
    

    private void OnDestroy()
    {
        BrokenCastleManager.OnBrokenCastle -= SetDestination;
    }
}
