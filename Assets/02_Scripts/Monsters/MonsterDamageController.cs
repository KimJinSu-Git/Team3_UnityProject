using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDamageController : MonoBehaviour, IDamageAble
{
    public GameObject GameObject => gameObject;
    public Collider Collider => MainCollider;
    
    
    private int currentHp;
    
    private Collider MainCollider;
    [SerializeField] MonsterData monsterData;
    private void Start()
    {
        TryGetComponent(out MainCollider);
        currentHp = monsterData.maxHP;
    }

    public void TakeDamage(int combatEvent, bool OnDamage)
    {   
        currentHp -= combatEvent;
        Debug.Log($"아파용 {monsterData.monsterName} : HP = {currentHp}/{monsterData.maxHP}"); 
        
        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{monsterData.monsterName}이 죽었어요");
        Destroy(gameObject);
    }
}