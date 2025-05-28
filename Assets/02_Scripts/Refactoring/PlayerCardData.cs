using System;
using UnityEngine;

[System.Serializable]
public class PlayerCardData
{
    public string id;           // 카드 ID (예: "orc001")
    public int level;           // 현재 카드 레벨
    public int ownedCount;      // 보유 중인 카드 수량

    [NonSerialized] public MonsterData monsterData; // 런타임에서 연결
    [NonSerialized] public SkillData skillData; // 런타임에서 연결

    public PlayerCardData(string id, int level, int ownedCount = 0)
    {
        this.id = id;
        this.level = level;
        this.ownedCount = ownedCount;
    }
}