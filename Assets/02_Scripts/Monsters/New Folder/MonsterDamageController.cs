// using System;
// using System.Collections;
// using System.Collections.Generic;
// using Fusion;
// using UnityEngine;
//
// public class MonsterDamageController : NetworkBehaviour, IDamageAble
// {
//     public GameObject GameObject => gameObject;
//     public Collider Collider => MainCollider;
//     public void TakeDamage(int damage)
//     {
//         throw new NotImplementedException();
//     }
//
//     private Collider MainCollider;
//     
//     [SerializeField] MonsterData monsterData;
//     private Stat MonsterStat = new Stat();
//     private void Start()
//     {
//         TryGetComponent(out MainCollider);
//         MonsterStat.maxHP = monsterData.maxHP;
//         MonsterStat.level = monsterData.level;
//         MonsterStat.currentHp = (float)monsterData.maxHP;
//         MonsterStat.Damage = monsterData.damage;
//     }
//
//     public void TakeDamage(int combatEvent, bool OnDamage)
//     {   
//         MonsterStat.currentHp -= combatEvent;
//         Debug.Log($"아파용 {monsterData.monsterName} : HP = {MonsterStat.currentHp}/{monsterData.maxHP}"); 
//         
//         if (MonsterStat.currentHp <= 0)
//         {
//             Die();
//         }
//     }
//
//     private void Die()
//     {
//         Debug.Log($"{monsterData.monsterName}이 죽었어요");
//         Destroy(gameObject);
//     }
// }
//
// [System.Serializable]
// public class Stat
// {
//     public int maxHP;
//     public int level;
//     public float currentHp;
//     public float Damage;
// }