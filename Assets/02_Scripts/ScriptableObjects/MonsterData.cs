using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "ScriptableObjects/MonsterData")]
public class MonsterData : ScriptableObject
{
    public enum AttackType
    {
        AttackNull,
        FlyAttack,
        OnlyGroundAttack,
        FlyWalkAttack,
    }
    [Header("BaseInfo")]
    public string id;
    public string monsterName;   
    public string description;
    
    [Header("Stat")] 
    public int level;
    public int maxHP;
    public float damage;
    public float moveSpeed;
    public float attackRange;
    public float attackSpeed;
    public AttackType attack;
    [Header("SpawnInfo")]
    public int cost;
    public float spawnTime;
    
    [Header("Rendering")]
    public GameObject prefab;
    public Sprite icon;
}
