using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "MonsterData", menuName = "ScriptableObjects/MonsterData")]
public class MonsterData : ScriptableObject
{
    public enum CharacterType
    {
        //Fly,
        NearAttack,
        FarAttack,
        Building
    }
    [Header("BaseInfo")]
    public string id;
    public string monsterName;   
    public string description;
    public CharacterType characterType;
    
    [Header("Ranged Attack (FarAttack 전용)")]
    public GameObject projectilePrefab;     // 발사체 프리팹
    public float projectileSpeed = 8f;     // 투사체 속도
    // public Transform shootOffset;          // 발사 위치 필요 시
    
    [Header("Stat")] 
    public int level;
    public int maxHP;
    public int damage;
    public float moveSpeed;
    public float attackRange;
    public float detectionRadius;
    public float attackSpeed;
    
    [Header("SpawnInfo")]
    public int cost;
    public float spawnTime;
    
    [Header("Rendering")]
    public GameObject prefab;
    public GameObject previewPrefab;
    public Sprite icon;
}
