using System.Collections;
using Fusion;
using UnityEngine;
using UnityEngine.AI;

public abstract class BaseMonster : NetworkBehaviour, IDamageAble
{
    public enum MonsterState { Idle, ChaseTarget, Attack, Die }

    public GameObject GameObject => gameObject;
    public Collider Collider => collider;
    public PlayerRef PlayerRef => playerRef;
    public NetworkObject NetworkObject => Object;
    public bool IsAlive => CurrentHp > 0;

    public PlayerRef playerRef;
    public MonsterData monsterData;

    protected Collider collider;
    protected NavMeshAgent agent;
    protected Animator animator;

    protected float maxHp;
    [Networked] public float CurrentHp { get; set; }

    protected bool isDie = false;
    protected MonsterState monsterState = MonsterState.Idle;

    protected IDamageAble currentTarget;
    protected Vector3 targetPoint;
    protected float attackTimer = 0f;

    protected Renderer[] renderers;
    protected Color[] originalColors;
    protected Coroutine hitEffectCoroutine;

    protected virtual void Awake()
    {
        TryGetComponent(out collider);
        TryGetComponent(out agent);
        TryGetComponent(out animator);

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
        CombatSystem.Instance.RegisterCreature(collider, this);
        maxHp = monsterData.maxHP;
        CurrentHp = maxHp;
        agent.speed = monsterData.moveSpeed;
    }

    public override void Spawned()
    {
        // if (playerRef != UserManager.Instance.FusionPlayerRef)
        // {
        //     Vector3 pos = transform.position;
        //     pos.x = -pos.x;
        //     pos.y = -pos.y;
        //     transform.position = pos;
        // }
        //
        // if (Object.HasInputAuthority == false)
        // {
        //     Vector3 pos = transform.position;
        //     pos.x = -pos.x;
        //     pos.y = -pos.y;
        //     transform.position = pos;
        // }
    }
    public override void FixedUpdateNetwork()
    {
        if (isDie) return;

        switch (monsterState)
        {
            case MonsterState.Idle:
                PlayAnimation("Idle");
                DetectTarget();
                break;
            case MonsterState.ChaseTarget:
                PlayAnimation("Walk");
                if (currentTarget != null)
                {
                    targetPoint = currentTarget.GameObject.transform.position;
                    agent.SetDestination(targetPoint);
                    float distance = Vector3.Distance(transform.position, targetPoint);
                    if (distance <= monsterData.attackRange)
                    {
                        monsterState = MonsterState.Attack;
                        agent.ResetPath();
                    }
                }
                else monsterState = MonsterState.Idle;
                break;
            case MonsterState.Attack:
                if (currentTarget == null || !currentTarget.IsAlive)
                {
                    monsterState = MonsterState.Idle;
                    return;
                }
                PlayAnimation("Attack");
                TryAttack();
                break;
        }
    }

    protected virtual void PlayAnimation(string animName)
    {
        if (animator == null || animator.GetCurrentAnimatorStateInfo(0).IsName(animName)) return;
        animator.CrossFade(animName, 0.1f);
    }

    protected virtual void DetectTarget()
    {
        IDamageAble bestTarget = FindClosestTarget("Tower", 200f);

        if (monsterData.targetPriority == MonsterData.TargetPriorityType.UnitAndTower)
        {
            IDamageAble unitTarget = FindClosestTarget("Monster", 5f);
            if (unitTarget != null)
            {
                bestTarget = unitTarget;
            }
        }

        if (bestTarget != null)
        {
            currentTarget = bestTarget;
            targetPoint = currentTarget.GameObject.transform.position;
            agent.SetDestination(targetPoint);
            monsterState = MonsterState.ChaseTarget;
        }
    }

    protected virtual IDamageAble FindClosestTarget(string layerName, float radius)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, LayerMask.GetMask(layerName));
        NavMeshPath path = new NavMeshPath();

        float closestDist = float.MaxValue;
        IDamageAble result = null;

        foreach (var col in colliders)
        {
            IDamageAble damageable = col.GetComponent<IDamageAble>();
            if (damageable == null || damageable == this) continue;
            if (damageable.PlayerRef == this.PlayerRef) continue;

            Vector3 point = col.ClosestPoint(transform.position);
            if (!NavMesh.CalculatePath(transform.position, point, NavMesh.AllAreas, path)) continue;

            float dist = GetDistance(path);
            if (path.status == NavMeshPathStatus.PathComplete && dist < closestDist)
            {
                closestDist = dist;
                result = damageable;
            }
        }
        return result;
    }

    protected virtual void TryAttack()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= monsterData.attackSpeed)
        {
            if (currentTarget != null && currentTarget.PlayerRef != this.PlayerRef)
            {
                CombatEvent combatEvent = new CombatEvent
                {
                    Sender = this,
                    Receiver = currentTarget,
                    Damage = Mathf.RoundToInt(monsterData.damage),
                    UseEffect = true,
                    EffectName = "HitEffect",
                    EffectPosition = currentTarget.GameObject.transform.position,
                    NetworkObject = currentTarget.NetworkObject
                };

                CombatSystem.Instance.AddCombatEvent(combatEvent);
            }
            attackTimer = 0f;
        }
    }

    protected float GetDistance(NavMeshPath path)
    {
        float distance = 0f;
        if (path.corners.Length < 2) return distance;
        for (int i = 1; i < path.corners.Length; i++)
        {
            distance += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }
        return distance;
    }

    public virtual void TakeDamage(int damage)
    {
        if (isDie) return;

        CurrentHp -= damage;

        if (hitEffectCoroutine != null)
            StopCoroutine(hitEffectCoroutine);
        hitEffectCoroutine = StartCoroutine(HitFlash());

        if (CurrentHp <= 0)
        {
            isDie = true;
            monsterState = MonsterState.Die;
            RPC_OnDie();
        }
    }

    protected virtual IEnumerator HitFlash()
    {
        foreach (var rend in renderers)
        {
            rend.material.color = Color.red;
        }

        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = originalColors[i];
        }
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public virtual void RPC_OnDie()
    {
        if (Object.HasStateAuthority == false) return;
        Runner.Despawn(Object);
    }
}
