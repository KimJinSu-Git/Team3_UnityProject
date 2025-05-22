using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeRequirementDB", menuName = "ScriptableObjects/UpgradeRequirementDB")]
public class UpgradeRequirementDB : ScriptableObject
{
    public List<RarityLevelInfo> rarityRequirements;

    public const int MAX_LEVEL = 11;

    public int GetRequiredCards(MonsterData_Mainmenu.Rarity rarity, int currentLevel)
    {
        var data = rarityRequirements.Find(r => r.rarity == rarity);

        if (data == null || currentLevel < 1 || currentLevel >= MAX_LEVEL)
            return int.MaxValue;

        return data.cardsNeededPerLevel[currentLevel - 1];
    }

    public int GetRequiredGold(MonsterData_Mainmenu.Rarity rarity, int currentLevel)
    {
        var data = rarityRequirements.Find(r => r.rarity == rarity);

        if (data == null || currentLevel < 1 || currentLevel >= MAX_LEVEL)
            return int.MaxValue;

        return data.goldNeededPerLevel[currentLevel - 1];
    }

    public bool CanUpgrade(MonsterData_Mainmenu monster)
    {
        return monster.level < MAX_LEVEL;
    }
}