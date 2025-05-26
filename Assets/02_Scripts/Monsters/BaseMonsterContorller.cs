using Fusion;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// 데이터 드리븐 방식 + 애니메이션/CombatSystem 연동: 몬스터 유닛 컨트롤러
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class BaseMonsterController : NetworkBehaviour, IDamageAble
{
    public GameObject GameObject => gameObject;
    public Collider Collider => GetComponent<Collider>();
    public PlayerRef PlayerRef => playerRef;
    public NetworkObject NetworkObject => Object;

    [Header("Data")]
    public MonsterData monsterData;

    [Header("Owner")]
    public PlayerRef playerRef;

    [Header("Detection Ranges")]
    [SerializeField] private float unitAggroRadius = 3f;
    [SerializeField] private float towerDetectRadius = 40f;

    protected NavMeshAgent agent;
    protected Animator animator;
    protected Renderer[] renderers;
    protected Color[] originalColors;

    protected Transform currentTarget;
    protected float attackTimer;
    public bool isDead = false;
    [Networked] public float currentHp { get; set; }

    protected enum State { Idle, Moving, Attacking }
    protected State currentState = State.Idle;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material = Instantiate(renderers[i].material);
            originalColors[i] = renderers[i].material.color;
        }
    }

    protected virtual void Start()
    {
        if (monsterData != null)
        {
            agent.speed = monsterData.moveSpeed;
            agent.autoBraking = false;
            currentHp = monsterData.maxHP;
        }

        var col = GetComponent<Collider>();
        if (CombatSystem.Instance != null)
            CombatSystem.Instance.RegisterCreature(col, this);
        
        if (MonsterHealthBarManager.Instance != null)
            MonsterHealthBarManager.Instance.Register(this, monsterData.maxHP);
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority || isDead) return;

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

    protected virtual void UpdateTarget()
    {
        var unit = FindNearestEnemy(unitAggroRadius, LayerMask.GetMask("Monster"));
        if (unit != null)
        {
            SetNewTarget(unit);
            return;
        }
        var tower = FindNearestEnemy(towerDetectRadius, LayerMask.GetMask("Tower"));
        if (tower != null)
        {
            SetNewTarget(tower);
            return;
        }
        currentTarget = null;
        currentState = State.Idle;
        PlayAnimation("Idle");
    }

    protected virtual Transform FindNearestEnemy(float radius, int layerMask)
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

    protected virtual void SetNewTarget(Transform target)
    {
        currentTarget = target;
        currentState = State.Moving;
        attackTimer = 0f;
        PlayAnimation("Walk");
    }

    protected virtual void UpdateMovement()
    {
        if (currentTarget == null)
        {
            currentState = State.Idle;
            PlayAnimation("Idle");
            return;
        }

        float dist = Vector3.Distance(transform.position, currentTarget.position);
        if (dist <= monsterData.attackRange)
        {
            agent.isStopped = true;
            currentState = State.Attacking;
            attackTimer = 0f;
            PlayAnimation("Attack");
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(currentTarget.position);
        }
    }

    protected virtual void UpdateAttack()
    {
        if (currentTarget == null)
        {
            currentState = State.Idle;
            PlayAnimation("Idle");
            return;
        }
        
        Vector3 direction = (currentTarget.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        attackTimer += Runner.DeltaTime;
        if (attackTimer >= monsterData.attackSpeed)
        {
            attackTimer = 0f;
            if (currentTarget.TryGetComponent<IDamageAble>(out var dmg))
            {
                CombatEvent combatEvent = new CombatEvent
                {
                    Sender = this,
                    Receiver = dmg,
                    Damage = monsterData.damage,
                    UseEffect = true,
                    EffectName = "HitEffect",
                    EffectPosition = dmg.GameObject.transform.position,
                    NetworkObject = dmg.NetworkObject
                };
                CombatSystem.Instance.AddCombatEvent(combatEvent);
                OnAttackEffect();
            }
        }

        float dist = Vector3.Distance(transform.position, currentTarget.position);
        if (dist > monsterData.attackRange)
        {
            currentState = State.Moving;
            PlayAnimation("Walk");
        }
    }

    protected virtual void PlayAnimation(string animName)
    {
        if (animator == null) return;

        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName(animName) && stateInfo.normalizedTime < 0.8f) return;

        animator.Play(animName);
    }

    protected virtual void OnAttackEffect()
    {
        // 이펙트나 타격 파티클 처리 오버라이드 용도
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHp -= damage;
        MonsterHealthBarManager.Instance.UpdateHealth(this, currentHp);
        StartCoroutine(HitFlash());

        if (currentHp <= 0)
        {
            MonsterHealthBarManager.Instance.Unregister(this);
            Die();
        }
    }

    private IEnumerator HitFlash()
    {
        foreach (var rend in renderers)
            rend.material.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < renderers.Length; i++)
            renderers[i].material.color = originalColors[i];
    }

    protected virtual void Die()
    {
        isDead = true;
        PlayAnimation("Die");
        RPC_OnDie();
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    protected virtual void RPC_OnDie()
    {
        if (!Object.HasStateAuthority) return;
        Runner.Despawn(Object);
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmosSelected()
    {
        if (monsterData != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, monsterData.attackRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, unitAggroRadius);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, towerDetectRadius);
        }
    }
#endif
}