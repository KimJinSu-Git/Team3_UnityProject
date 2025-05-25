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
