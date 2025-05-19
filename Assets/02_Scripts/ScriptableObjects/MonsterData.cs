using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "ScriptableObjects/MonsterData")]
public class MonsterData : ScriptableObject
{
    [Header("BaseInfo")]
    public string id;
    public string monsterName;   
    public string description;
    
    [Header("Stat")]
    public int maxHP;
    public float damage;
    public float moveSpeed;
    public float attackSpeed;
    
    [Header("SpawnInfo")]
    public int cost;
    public float spawnTime;
    public float lifeTime;
    
    [Header("Rendering")]
    public GameObject prefab;
    public Sprite icon;
}
