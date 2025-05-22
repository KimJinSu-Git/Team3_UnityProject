using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData_MainMenu", menuName = "ScriptableObjects/MonsterData_MainMenu")]
public class MonsterData_Mainmenu : ScriptableObject
{
    public enum AttackType { AttackNull, FlyAttack, OnlyGroundAttack, FlyWalkAttack }
    public enum CardType { Unit, Spell, Building }
    public enum Rarity { Common, Rare, Epic, Legendary }

    [Header("Base Info")]
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

    [Header("Type Info")]
    public CardType cardType;
    public Rarity rarity;

    [Header("Spawn Info")]
    public int cost;
    public float spawnTime;

    [Header("Rendering")]
    public GameObject prefab;
    public GameObject previewPrefab;
    public Sprite icon;

    [Header("Utility")]
    public int displayOrder;    // UI 정렬용
    public string keyword;      // 검색용
}