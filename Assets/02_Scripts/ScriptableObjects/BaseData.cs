using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

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
    public string name;   
    public string description;
    public int cost;
    public GameObject prefab;
    public Sprite icon;
    public CardDataType cardDataType;
    public Rarity rarity= Rarity.Common;
}
