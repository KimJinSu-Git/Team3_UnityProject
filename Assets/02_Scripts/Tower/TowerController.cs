using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TerrainTools;

public class TowerController : MonoBehaviour, IDamageAble
{
    public enum TowerType { LeftPrincess, RightPrincess, King }

    [Header("타워 정보")]
    public TowerType towerType;
    public int maxHealth = 2000;
    [SerializeField] private int currentHealth;

    [Header("감지 대상")]
    public LayerMask targetLayer;

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
    
    [Header("HP")]
    public HealthBar healthBar;
    
    private float attackTimer = 0f;
    private Transform target;
    private PlayerRef playerRef;
    

    public GameObject GameObject => this.gameObject;
    public Collider Collider { get; private set; }
    public PlayerRef PlayerRef => playerRef; // 처음에 스폰해서 ref할당
    public NetworkObject NetworkObject { get; }

    public bool IsDead;
    
    public bool IsAlive => currentHealth > 0;
    
    public void ForceDestroy()
    {
        if (!IsAlive) return;

        currentHealth = 0;
        Die();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        Collider = GetComponent<Collider>();
        CombatSystem.Instance.RegisterCreature(Collider, this);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            TakeDamage(100);
        }
        
        if(IsDead)
        {
            Die();
        }
        
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackInterval)
        {
            target = FindNearestTarget();
            if (target != null)
            {
                attackTimer = 0f;
                archerAnimator?.Play("Shoot");
                StartCoroutine(FireArrowAfterDelay(fireDelay));
            }
            else
            {
                archerAnimator?.Play("Idle");
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentHealth -= 500;
            if (currentHealth <= 0)
                Die();
        }
    }

    private IEnumerator FireArrowAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (target == null) yield break;

        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);
        arrow.GetComponent<ArrowProjectile>().Init(target, attackDamage);
    }

    private Transform FindNearestTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, targetLayer);
        float minDist = float.MaxValue;
        Transform closest = null;

        foreach (var hit in hits)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = hit.transform;
            }
        }
        return closest;
    }
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);
        Debug.Log($"{towerType} 피해: {damage} / 현재 체력: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    public void Die()
    {
        Debug.Log($"{towerType} 파괴됨!");

        if (spawnCollider != null && spawnAreaImage != null)
        {
            spawnCollider.SetActive(false);
            spawnAreaImage.SetActive(false);
        }

        if (towerType == TowerType.King)
            GameManager.Instance.OnKingTowerDestroyed(this);
        else if (towerType == TowerType.LeftPrincess)
        {
            BrokenCastleManager.OnTriggerCastleBroken(1);
            GameManager.Instance.OnPrincessTowerDestroyed(this);
        }
        else if (towerType == TowerType.RightPrincess)
        {
            BrokenCastleManager.OnTriggerCastleBroken(2);
            GameManager.Instance.OnPrincessTowerDestroyed(this);
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
