using Fusion;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 데이터 드리븐 방식: MonsterData를 통해 스탯을 읽어와 타겟 탐지 및 전투를 수행하는 몬스터 컨트롤러
/// </summary>
public class BaseMonsterController : NetworkBehaviour, IDamageAble
{
    // 네트워크 객체 참조
    public NetworkObject NetworkObject => Object;

    [Header("Data")]
    public MonsterData monsterData;

    [Header("Owner")]
    public PlayerRef playerRef;

    [Header("Detection Ranges")]
    [SerializeField] private float unitAggroRadius = 3f;
    [SerializeField] private float towerDetectRadius = 40f;

    private NavMeshAgent agent;
    private Transform currentTarget;
    private float attackTimer;
    private bool isDead;
    private float currentHp;

    private enum State { Idle, Moving, Attacking }
    private State currentState = State.Idle;

    void Awake()
    {
        // 스탯 초기화
        if (monsterData != null)
        {
            agent = GetComponent<NavMeshAgent>();
            agent.speed = monsterData.moveSpeed;
            agent.autoBraking = false;

            currentHp = monsterData.maxHP;
        }

        // CombatSystem 등록
        var col = GetComponent<Collider>();
        if (CombatSystem.Instance != null)
            CombatSystem.Instance.RegisterCreature(col, this);
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority || isDead)
            return;

        if (currentState != State.Attacking)
            UpdateTarget();

        switch (currentState)
        {
            case State.Idle:
            case State.Moving:
                UpdateMovement();
                break;
            case State.Attacking:
                UpdateAttack();
                break;
        }
    }

    private void UpdateTarget()
    {
        // 유닛 우선 탐지
        var unit = FindNearestEnemy(unitAggroRadius, LayerMask.GetMask("Monster"));
        if (unit != null)
        {
            SetNewTarget(unit);
            return;
        }
        // 타워 탐지
        var tower = FindNearestEnemy(towerDetectRadius, LayerMask.GetMask("Tower"));
        if (tower != null)
        {
            SetNewTarget(tower);
            return;
        }
        currentTarget = null;
        currentState = State.Idle;
    }

    private Transform FindNearestEnemy(float radius, int layerMask)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, layerMask);
        Transform nearest = null;
        float distMin = float.MaxValue;
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageAble>(out var dmg) && dmg.PlayerRef != playerRef)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < distMin)
                {
                    distMin = dist;
                    nearest = hit.transform;
                }
            }
        }
        return nearest;
    }

    private void SetNewTarget(Transform target)
    {
        currentTarget = target;
        currentState = State.Moving;
        attackTimer = 0f;
    }

    private void UpdateMovement()
    {
        if (currentTarget == null)
            return;

        float dist = Vector3.Distance(transform.position, currentTarget.position);
        if (dist <= monsterData.attackareaRange)
        {
            agent.isStopped = true;
            currentState = State.Attacking;
            attackTimer = 0f;
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(currentTarget.position);
        }
    }

    private void UpdateAttack()
    {
        if (currentTarget == null)
        {
            currentState = State.Idle;
            return;
        }
        attackTimer += Runner.DeltaTime;
        if (attackTimer >= monsterData.attackSpeed)
        {
            attackTimer = 0f;
            if (currentTarget.TryGetComponent<IDamageAble>(out var dmg))
            {
                dmg.TakeDamage(monsterData.damage);
            }
        }
        float dist = Vector3.Distance(transform.position, currentTarget.position);
        if (dist > monsterData.attackareaRange)
            currentState = State.Moving;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHp -= damage;
        if (currentHp <= 0)
            Die();
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    private void RPC_OnDie()
    {
        if (!Object.HasStateAuthority) return;
        Runner.Despawn(Object);
    }

    private void Die()
    {
        isDead = true;
        RPC_OnDie();
    }

    public GameObject GameObject => gameObject;
    public Collider Collider => GetComponent<Collider>();
    public PlayerRef PlayerRef => playerRef;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, monsterData.attackareaRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, unitAggroRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, towerDetectRadius);
    }
#endif
}