using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyAttacker : MonoBehaviour, IDamageAble
{
    [Header("감지 대상")]
    public LayerMask targetLayer;
    
    public float attackInterval = 1.5f;
    public int attackDamage = 100;
    private float attackTimer = 0f;

    private void Update()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackInterval)
        {
            TryAttackTower();
            attackTimer = 0f;
        }
    }

    private void TryAttackTower()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 3f, targetLayer); // 근처의 타워 감지
        foreach (var col in hits)
        {
            IDamageAble target = CombatSystem.Instance.GetCreatureOrNull(col);
            if (target != null && target != this)
            {
                CombatEvent newEvent = new CombatEvent();
                
                newEvent.Sender = this;
                newEvent.Receiver = target;
                newEvent.Damage = attackDamage;
                newEvent.Collider = col;
                newEvent.UseEffect = true;
                newEvent.EffectPosition = col.transform.position;
                newEvent.EffectName = "HitEffect";

                CombatSystem.Instance.AddCombatEvent(newEvent);
                Debug.Log($"[Dummy] 타워에 공격 이벤트 보냄: {attackDamage}");
                break;
            }
        }
    }

    public GameObject GameObject { get; }
    public Collider Collider { get; }
    public void TakeDamage(int combatEvent, bool OnDamage)
    {
        throw new System.NotImplementedException();
    }
}