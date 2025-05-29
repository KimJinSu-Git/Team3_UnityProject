using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TerrainTools;

public class TowerController : NetworkBehaviour, IDamageAble
{
    public enum TowerType { LeftPrincess, RightPrincess, King }

    [Header("타워 정보")]
    public TowerType towerType;
    public int maxHealth = 300;
    [Networked] public int currentHealth { get; set; }

    [Header("감지 대상")]
    public LayerMask targetLayer; // 이제 Monster 포함해야 함

    [Header("공격 관련 정보")]
    public float attackRange = 5f;
    public float attackInterval = 1.5f;
    public int attackDamage = 100;
    public float fireDelay = 0.3f;

    [Header("화살 관련 정보")]
    public GameObject arrowPrefab;
    public Transform firePoint;

    [Header("궁수 애니메이터")]
    public Animator archerAnimator;

    [Header("카드 스폰 불가 지역")]
    public GameObject spawnAreaImage;
    public GameObject spawnCollider;

    [Header("렌더러")]
    private Renderer[] renderers;
    private Color[] originalColors;
    
    [Header("hp 바")]
    public TowerHealthBar healthBar;
    public GameObject hpBarObj;
    private float attackTimer = 0f;
    private Transform target;
    public PlayerRef playerRef;
    public PlayerRef tempPlayerRef;

    public Collider collider;
    public GameObject GameObject => gameObject;
    public Collider Collider => collider;
    public NetworkObject NetworkObject => Object;
    public PlayerRef PlayerRef => playerRef;

    public bool IsDead;
    public bool IsAlive => currentHealth > 0;
    public int RefID;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material = Instantiate(renderers[i].material);
            originalColors[i] = renderers[i].material.color;
        }
    }

    public override void Spawned()
    {
        currentHealth = maxHealth;
        if (healthBar != null)
            healthBar.SetMaxHealth(maxHealth);
    }
    private void Start()
    {
        tempPlayerRef = UserManager.Instance.FusionPlayerRef;

        if (transform.position.z <= 0f)
        {
            playerRef = tempPlayerRef;
            RefID = playerRef.PlayerId;
        }
        else if (transform.position.z > 0f)
        {
            if (tempPlayerRef == SessionManager.Instance.CurrentGameRoomInfo.HostPlayer)
            {
                playerRef = SessionManager.Instance.CurrentGameRoomInfo.ClientPlayer;
                RefID = playerRef.PlayerId;
            }
            else if (tempPlayerRef == SessionManager.Instance.CurrentGameRoomInfo.ClientPlayer)
            {
                playerRef = SessionManager.Instance.CurrentGameRoomInfo.HostPlayer;
                RefID = playerRef.PlayerId;
            }
        }

        //currentHealth = maxHealth;
        collider = GetComponent<Collider>();
        CombatSystem.Instance.RegisterCreature(Collider, this);
    }

    public override void FixedUpdateNetwork()
    {
        RPC_TowerHelathBar();
        if (Input.GetKeyDown(KeyCode.R)) TakeDamage(100);
        if (IsDead) Die();

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            target = FindNearestTarget();
            if (target != null)
            {
                attackTimer = 0f;
                //archerAnimator?.Play("Shoot");
                Rpc_PlayAnimation("Shoot");
                StartCoroutine(FireArrowAfterDelay(fireDelay));
            }
            else
            {
                //archerAnimator?.Play("Idle");
                Rpc_PlayAnimation("Idle");

            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentHealth -= 500;
            if (currentHealth <= 0) Die();
        }
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void Rpc_PlayAnimation(string animName)
    {
        archerAnimator?.Play(animName);
    }
    private IEnumerator FireArrowAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (target == null) yield break;

        NetworkObject arrowObj = Runner.Spawn(arrowPrefab, firePoint.position, Quaternion.identity);
        if (arrowObj.TryGetComponent<ArrowProjectile>(out var projectile))
        {
            projectile.Init(target.position, playerRef, attackDamage);
        }
    }

    private Transform FindNearestTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, targetLayer);
        float minDist = float.MaxValue;
        Transform closest = null;

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageAble>(out var dmg) && dmg.PlayerRef != playerRef)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = hit.transform;
                }
            }
        }
        return closest;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        //StartCoroutine(HitFlash());
        Rpc_HitFlash();
        Debug.Log($"{towerType} 피해: {damage} / 현재 체력: {currentHealth}");

        //RPC_TowerHelathBar();
        // if (healthBar != null)
        //     healthBar.SetHealth(currentHealth);
        //
        // if (currentHealth <= 0) Die();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_TowerHelathBar()
    {
        if (healthBar != null)
            healthBar.SetHealth(currentHealth);
        
        if (currentHealth <= 0) Die();
    }
    private IEnumerator HitFlash()
    {
        foreach (var rend in renderers)
            rend.material.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < renderers.Length; i++)
            renderers[i].material.color = originalColors[i];
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_HitFlash()
    {
        StartCoroutine(HitFlash());
    }
    
    public void Die()
    {
        Debug.Log($"{towerType} 파괴됨!");

        if (spawnCollider != null && spawnAreaImage != null)
        {
            spawnCollider.SetActive(false);
            spawnAreaImage.SetActive(false);
        }

        if (hpBarObj != null)
        {
            hpBarObj.SetActive(false);
        }
           
        if (towerType == TowerType.King)
            GameManager.Instance.RPC_OnKingTowerDestroyed(this);
        else if (towerType == TowerType.LeftPrincess)
        {
            GameManager.Instance.RPC_OnPrincessTowerDestroyed(this);
        }
        else if (towerType == TowerType.RightPrincess)
        {
            GameManager.Instance.RPC_OnPrincessTowerDestroyed(this);
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    public void ForceDestroy()
    {
        if (!IsAlive) return;
        currentHealth = 0;
        Die();
    }
}