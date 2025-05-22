using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerController : MonoBehaviour, IDamageAble
{
    public enum TowerType { Princess, King }
    
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

    private float attackTimer = 0f;
    
    public GameObject GameObject => this.gameObject;
    public Collider Collider { get; private set; }
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
        Collider = GetComponent<Collider>();
        
        CombatSystem.Instance.RegisterCreature(Collider, this);
    }

    private void Update()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            TryAttackNearestUnit();
            attackTimer = 0f;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentHealth -= 500;
            
            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    private void TryAttackNearestUnit()
    {
        // 지훈이가 만든 유닛을 태그나 Layer로 탐색
        Collider[] targets = Physics.OverlapSphere(transform.position, attackRange, targetLayer);
        Debug.Log($"감지된 유닛 수: {targets.Length}");
        
        foreach (var targetCol in targets)
        {
            IDamageAble target = CombatSystem.Instance.GetCreatureOrNull(targetCol);
            if (target != null && target != this)
            {
                Debug.Log($"타워가 유닛 공격 시도");
                // CombatEvent 생성, CombatSystem에 전달
                CombatEvent newEvent = new CombatEvent();

                newEvent.Sender = this;
                newEvent.Receiver = target;
                newEvent.Damage = attackDamage;
                newEvent.Collider = targetCol;
                newEvent.UseEffect = true;
                newEvent.EffectPosition = targetCol.transform.position;
                newEvent.EffectName = "HitEffect";

                CombatSystem.Instance.AddCombatEvent(newEvent);
                break;

            }
        }
    }

    // 지훈이가 만든 유닛이 공격 시 TakeDamage 호출
    public void TakeDamage(int damage, bool playEffect)
    {
        currentHealth -= damage;
        Debug.Log($"{towerType} 피해: {damage} / 현재 체력: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        Debug.Log($"{towerType} 파괴됨!");

        if (towerType == TowerType.King)
        {
            GameManager.Instance.OnKingTowerDestroyed(this);
        }
        else
        {
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
