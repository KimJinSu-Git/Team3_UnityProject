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
        Move,
        Attack,
    }
    public GameObject GameObject => gameObject;
    public Collider Collider => collider;
    public PlayerRef PlayerRef => playerRef; // 소환할때 값 세팅됨
    public NetworkObject NetworkObject => Object;
    public PlayerRef playerRef;
    public MonsterData monsterData;
    private Collider collider;
    private NavMeshAgent agent;
    private Animator animator;
    private float maxHp;
    [Networked] public float CurrentHp { get; set; }
    //private float currentHp;
    private bool isDie = false;
    MonsterState monsterState = MonsterState.Idle;
    void Awake()
    {
        TryGetComponent(out collider);
        TryGetComponent(out agent);
        TryGetComponent(out animator);
    }
    void Start()
    {
        CombatSystem.Instance.RegisterCreature(collider, this);
        maxHp = monsterData.maxHP;
        CurrentHp = maxHp;
       
    }

    public virtual void FixedUpdateNetwork()
    {
        TowerDetect();
    }
    public override void Spawned()
    {
        Debug.Log("PlayerRef : "+ playerRef);
        Debug.Log("Runner.LocalPlayer : "+ Runner.LocalPlayer);
        if (playerRef != Runner.LocalPlayer)
        {
            Vector3 pos = transform.position;
            pos.x = -pos.x;
            pos.z = -pos.z;
            transform.position = pos;
        }
    }
    private void MonsterDetect()
    {
        NearObjectDetect(5f, LayerMask.GetMask("Monster"));
    }
    private void TowerDetect()
    {
        NearObjectDetect(50f, LayerMask.GetMask("Tower"));
    }
    private void NearObjectDetect(float detectRadius, LayerMask mask)
    {
        Collider[] colliders= Physics.OverlapSphere(transform.position, detectRadius, mask);
        if (colliders.Length > 0)
        {
            Collider nearTower = null;
            float closetDist = float.MaxValue;
            NavMeshPath path = new NavMeshPath();
            foreach (Collider collider in colliders)
            {
                if(collider.GetComponent<IDamageAble>().PlayerRef ==  Runner.LocalPlayer) continue;
                Vector3 targetPos = collider.ClosestPoint(transform.position);
                if (NavMesh.CalculatePath(transform.position, targetPos, NavMesh.AllAreas, path)) // 경로 넣어줌
                {
                    float currentDistance = GetDistance(path);
                    if ((path.status == NavMeshPathStatus.PathComplete) && currentDistance < closetDist)
                    {
                        closetDist = currentDistance;
                        nearTower = collider;
                    }
                }
            }
            if (nearTower != null)
            {
                Vector3 ClosetPos = nearTower.ClosestPoint(transform.position);
                agent.SetDestination(ClosetPos);
            }
        }
    }
    private float GetDistance(NavMeshPath path)
    {
        float distance = 0f;
        if (path.corners.Length < 2) return distance; // 경로가 없으면

        for (int i = 1; i < path.corners.Length; i++)
        {
            distance += Vector3.Distance(path.corners[i-1], path.corners[i]);
        }

        return distance;

    }
    public void TakeDamage(int damage)
    {
        if (isDie == false)
        {
            CurrentHp -= damage;
            if (CurrentHp <= 0)
            {
                RPC_OnDie();
            }
            isDie = true;
        }
    }
    // 풀링으로 바꿀예정
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_OnDie()
    {
        if (Object.HasStateAuthority == false) return;
        Runner.Despawn(Object); // Destroy안해도 Fusion이 자동으로 파괴
    }
}
