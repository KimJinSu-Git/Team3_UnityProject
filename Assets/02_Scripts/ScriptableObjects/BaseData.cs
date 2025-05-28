using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.Serialization;

public enum CardDataType
{
    Monster,
    Skill
}
public enum Rarity
{
    Common,
    Rare,
    Epic,
    Legendary
}
public class BaseData : ScriptableObject
{
    public string id;
    public string cardName;   
    public string description;
    public int cost;
    public int damage;
    public GameObject prefab;
    public Sprite icon;
    public CardDataType cardDataType;
    public Rarity rarity= Rarity.Common;
}
