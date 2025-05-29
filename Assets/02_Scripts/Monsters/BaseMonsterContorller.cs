using Fusion;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// 데이터 드리븐 방식 + 애니메이션/CombatSystem 연동: 몬스터 유닛 컨트롤러 (근거리 & 원거리 통합)
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
    
    [Networked]public PlayerRef playerRef{get;set;}

    [Header("Detection Ranges")]
    [SerializeField] private float unitAggroRadius = 3f;
    [SerializeField] private float towerDetectRadius = 40f;
    
    [Header("attack animation")]
    [SerializeField] private float attackImpactNormalizedTime = 0.6f;
    protected bool hasDealtDamage; 

    protected NavMeshAgent agent;
    protected Animator animator;
    protected Renderer[] renderers;
    protected Color[] originalColors;

    protected Transform currentTarget;
    protected float attackTimer;
    public bool isDead = false;
    [Networked] protected float currentHp { get; set; }

    protected enum State { Idle, Moving, Attacking }
    protected State currentState = State.Idle;
    private bool isInitialized = false;

    protected virtual void Awake()
    {
        // agent = GetComponent<NavMeshAgent>();
        // animator = GetComponent<Animator>();
        //
        // renderers = GetComponentsInChildren<Renderer>();
        // originalColors = new Color[renderers.Length];
        //
        // for (int i = 0; i < renderers.Length; i++)
        // {
        //     renderers[i].material = Instantiate(renderers[i].material);
        //     originalColors[i] = renderers[i].material.color;
        // }
    }

    public override void Spawned()
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

        isInitialized = true;
        // Start
        if (!Object.HasStateAuthority)
        {
            agent.enabled = false;
        }

        if (monsterData != null)
        {
            agent.speed = monsterData.moveSpeed;
            agent.autoBraking = false;
            currentHp = monsterData.maxHP;
        }

        if (CombatSystem.Instance != null)
            CombatSystem.Instance.RegisterCreature(GetComponent<Collider>(), this);

        if (MonsterHealthBarManager.Instance != null)
            MonsterHealthBarManager.Instance.Register(this, monsterData.maxHP);
    }
    // protected virtual void Start()
    // {
    //     if (Object.HasStateAuthority == false) 
    //     {
    //         GetComponent<NavMeshAgent>().enabled = false;
    //     }
    //     if (monsterData != null)
    //     {
    //         agent.speed = monsterData.moveSpeed;
    //         agent.autoBraking = false;
    //         currentHp = monsterData.maxHP;
    //     }
    //
    //     var col = GetComponent<Collider>();
    //     if (CombatSystem.Instance != null)
    //         CombatSystem.Instance.RegisterCreature(col, this);
    //     
    //     if (MonsterHealthBarManager.Instance != null)
    //         MonsterHealthBarManager.Instance.Register(this, monsterData.maxHP);
    // }

    public override void FixedUpdateNetwork()
    {
        //if (!Object.HasStateAuthority || isDead) return;

        if (!Object || !Object.HasStateAuthority || isDead || !isInitialized ) return;
        RPC_HPUI();
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
        Rpc_PlayAnimation("Idle");
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
        Rpc_PlayAnimation("Walk");
    }

    protected virtual void UpdateMovement()
    {
        if (currentTarget == null)
        {
            currentState = State.Idle;
            Rpc_PlayAnimation("Idle");
            return;
        }

        float dist = Vector3.Distance(transform.position, currentTarget.position);
        if (dist <= monsterData.attackRange)
        {
            agent.isStopped = true;
            currentState = State.Attacking;
            attackTimer = 0f;
            Rpc_PlayAnimation("Attack");
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(currentTarget.position);
        }
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void Rpc_PlayAnimation(string animName)
    {
        if (Object == null) return;
        PlayAnimation(animName);
    }
    
    
     protected virtual void UpdateAttack()
    {
        if(isDead) return;
        if (currentTarget == null)
        {
            currentState = State.Idle;
            Rpc_PlayAnimation("Idle");
            return;
        }

        // 1) 타겟 바라보기
        Vector3 dir = currentTarget.position - transform.position;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir.normalized), Time.deltaTime * 10f);
        
        // 2) 이동 멈추기
        agent.isStopped = true;

        // 3) Attack 애니메이션 상태 읽기
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName("Attack"))
        {
            // 첫 진입 시 애니메이션 전파 및 플래그 초기화
            hasDealtDamage = false;
            Rpc_PlayAnimation("Attack");
            return;
        }

        // 4) normalizedTime % 1 로 매 사이클 진행도 계산
        float cycleTime = stateInfo.normalizedTime % 1f;

        // 4-1) 지정 시점에 데미지 한 번
        if (!hasDealtDamage && cycleTime >= attackImpactNormalizedTime)
        {
            hasDealtDamage = true;
            if (monsterData.characterType == MonsterData.CharacterType.FarAttack)
                FireProjectile();
            else
                DoDealDamage();
        }

        // 4-2) 다음 사이클 대비 플래그 리셋
        if (hasDealtDamage && cycleTime < attackImpactNormalizedTime)
        {
            hasDealtDamage = false;
        }

        // 5) 사거리 벗어나면 이동 상태로 복귀
        float dist = Vector3.Distance(transform.position, currentTarget.position);
        if (dist > monsterData.attackRange)
        {
            currentState = State.Moving;
            agent.isStopped = false;
            Rpc_PlayAnimation("Walk");
        }
    }
     
    protected virtual void FireProjectile()
    {
        if (monsterData.projectilePrefab == null || currentTarget == null) return;

        // 투사체가 NetworkObject를 포함하고 있어야 함
        var netObj = monsterData.projectilePrefab.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("❌ projectilePrefab에 NetworkObject가 없습니다.");
            return;
        }

        // spawn 위치: 몬스터의 위치에서 위로 약간 띄운 지점
        Vector3 spawnPos = transform.position + Vector3.up;
        NetworkObject spawned = Runner.Spawn(netObj, spawnPos, Quaternion.identity, Object.InputAuthority);

        // RPC 방식의 Init 호출
        if (spawned.TryGetComponent<MonsterProjectile>(out var proj))
        {
            proj.Init(currentTarget, playerRef, monsterData.damage, monsterData.projectileSpeed);
        }
    }
    
    protected virtual void DoDealDamage()
    {
        if (currentTarget.TryGetComponent<IDamageAble>(out var dmg))
        {
            CombatEvent ev = new CombatEvent
            {
                Sender = this,
                Receiver = dmg,
                Damage = monsterData.damage,
                UseEffect = true,
                EffectName = "HitEffect",
                EffectPosition = dmg.GameObject.transform.position,
                NetworkObject = dmg.NetworkObject
            };
            CombatSystem.Instance.AddCombatEvent(ev);
            OnAttackEffect();
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
        
        //MonsterHealthBarManager.Instance.UpdateHealth(this, currentHp);
        
        //StartCoroutine(HitFlash());
        Rpc_HitFlash();
        // if (currentHp <= 0)
        // {
        //     Die();
        // }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_HPUI()
    {
        MonsterHealthBarManager.Instance.UpdateHealth(this, currentHp);
        if (currentHp <= 0)
        {
            Die();
            MonsterHealthBarManager.Instance.Unregister(this);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_HitFlash()
    {
        StartCoroutine(HitFlash());
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
        Rpc_PlayAnimation("Die");
        DeathVFXPool.Instance.Spawn(transform.position);
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