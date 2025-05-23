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
    
    public enum TargetPriorityType
    {
        TowerOnly,          // 타워만 바라보는 해바라기 사랑꾼
        UnitAndTower       // 자기를 바라봐주는 친구에게 맘이 변하는 금사빠
    }
    
    [Header("AI Behavior")]
    public TargetPriorityType targetPriority;
    
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
    public GameObject previewPrefab;
    public Sprite icon;
}
