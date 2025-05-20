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
    public bool isSelected;
    
    [Header("Stat")]
    public int maxHP;
    public float damage;
    public float moveSpeed;
    public float attackSpeed;
    public float attackRange;

    
    [Header("SpawnInfo")]
    public int cost;
    public float spawnTime;
    
    [Header("Rendering")]
    public GameObject prefab;
    public Sprite icon;
}
