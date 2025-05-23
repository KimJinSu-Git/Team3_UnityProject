using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.AI;

public class BaseMonster : NetworkBehaviour, IDamageAble
{
    enum MonsterState
    {
        Idle,
        ChaseTarget,
        Attack,
        Die
    }
    public GameObject GameObject => gameObject;
    public Collider Collider => collider;
    public PlayerRef PlayerRef => playerRef;
    public NetworkObject NetworkObject => Object;
    public bool IsAlive => CurrentHp > 0;

    public PlayerRef playerRef;
    public MonsterData monsterData;

    private Collider collider;
    private NavMeshAgent agent;
    private Animator animator;

    private float maxHp;
    [Networked] public float CurrentHp { get; set; }

    private bool isDie = false;
    private MonsterState monsterState = MonsterState.Idle;

    private IDamageAble currentTarget;
    private Vector3 targetPoint;
    private float attackTimer = 0f;
    
    private Renderer[] renderers;
    private Color[] originalColors;
    private Coroutine hitEffectCoroutine;
    
    void Awake()
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
    void Start()
    {
        CombatSystem.Instance.RegisterCreature(collider, this);
        maxHp = monsterData.maxHP;
        CurrentHp = maxHp;
        agent.speed = monsterData.moveSpeed;
    }

    // Fusion 네트워크에서 프레임마다 호출되는 메서드 (Update문)
    public virtual void FixedUpdateNetwork()
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
                else
                {
                    monsterState = MonsterState.Idle;
                }
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
        
        // TowerDetect();
    }
    
    private void PlayAnimation(string animName)
    {
        if (animator == null || animator.GetCurrentAnimatorStateInfo(0).IsName(animName))
            return;

        animator.CrossFade(animName, 0.1f); 
    }
    
    // 타겟 탐색 메서드
    private void DetectTarget()
    {
        // 타워 우선 탐지
        IDamageAble bestTarget = FindClosestTarget("Tower", 50f);

        // 유닛 조건부 탐지
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
    
    // 최적화된 경로 기반 가장 가까운 대상을 탐색하는 메서드임다.
    private IDamageAble FindClosestTarget(string layerName, float radius)
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
    
    private void TryAttack()
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
    
    private float GetDistance(NavMeshPath path)
    {
        float distance = 0f;
        if (path.corners.Length < 2) return distance;

        for (int i = 1; i < path.corners.Length; i++)
        {
            distance += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }

        return distance;
    }

    public void TakeDamage(int damage)
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
    
    private IEnumerator HitFlash()
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
    public void RPC_OnDie()
    {
        if (Object.HasStateAuthority == false) return;
        Runner.Despawn(Object);
    }

    public override void Spawned()
    {
        // if (playerRef != Runner.LocalPlayer)
        // {
        //     Vector3 pos = transform.position;
        //     pos.x = -pos.x;
        //     pos.y = -pos.y;
        //     transform.position = pos;
        // }
    }
    
    // private void MonsterDetect()
    // {
    //     NearObjectDetect(5f, LayerMask.GetMask("Monster"));
    // }
    // private void TowerDetect()
    // {
    //     NearObjectDetect(50f, LayerMask.GetMask("Tower"));
    // }
    // private void NearObjectDetect(float detectRadius, LayerMask mask)
    // {
    //     Collider[] colliders= Physics.OverlapSphere(transform.position, detectRadius, mask);
    //     if (colliders.Length > 0)
    //     {
    //         Collider nearTower = null;
    //         float closetDist = float.MaxValue;
    //         NavMeshPath path = new NavMeshPath();
    //         foreach (Collider collider in colliders)
    //         {
    //             if(collider.GetComponent<IDamageAble>().PlayerRef ==  Runner.LocalPlayer) continue;
    //             Vector3 targetPos = collider.ClosestPoint(transform.position);
    //             if (NavMesh.CalculatePath(transform.position, targetPos, NavMesh.AllAreas, path)) // 경로 넣어줌
    //             {
    //                 float currentDistance = GetDistance(path);
    //                 if ((path.status == NavMeshPathStatus.PathComplete) && currentDistance < closetDist)
    //                 {
    //                     closetDist = currentDistance;
    //                     nearTower = collider;
    //                 }
    //             }
    //         }
    //         if (nearTower != null)
    //         {
    //             Vector3 ClosetPos = nearTower.ClosestPoint(transform.position);
    //             agent.SetDestination(ClosetPos);
    //         }
    //     }
    // }
}
