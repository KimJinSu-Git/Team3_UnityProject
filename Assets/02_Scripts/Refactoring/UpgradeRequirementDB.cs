// using System.Collections.Generic;
// using UnityEngine;
//
// [System.Serializable]
// public class LevelRequirement
// {
//     public int level;
//     public int requiredCards;
//     public int requiredGold;
// }
//
// [System.Serializable]
// public class RarityLevelInfo
// {
//     public BaseData rarity;
//     public int startLevel = 1;
//     public List<LevelRequirement> levelRequirements;
// }
//
// [CreateAssetMenu(fileName = "UpgradeRequirementDB", menuName = "ScriptableObjects/UpgradeRequirementDB")]
// public class UpgradeRequirementDB : ScriptableObject
// {
//     public List<RarityLevelInfo> rarityRequirements;
//
//     public const int MAX_LEVEL = 11;
//
//     public int GetStartLevel(MonsterData.Rarity rarity)
//     {
//         var info = rarityRequirements.Find(r => r.rarity == rarity);
//         return info != null ? info.startLevel : 1;
//     }
//
//     public int GetRequiredCards(MonsterData.Rarity rarity, int level)
//     {
//         var info = rarityRequirements.Find(r => r.rarity == rarity);
//         var levelReq = info?.levelRequirements.Find(r => r.level == level);
//         return levelReq?.requiredCards ?? int.MaxValue;
//     }
//
//     public int GetRequiredGold(MonsterData.Rarity rarity, int level)
//     {
//         var info = rarityRequirements.Find(r => r.rarity == rarity);
//         var levelReq = info?.levelRequirements.Find(r => r.level == level);
//         return levelReq?.requiredGold ?? int.MaxValue;
//     }
//
//     public bool CanUpgrade(int currentLevel)
//     {
//         return currentLevel < MAX_LEVEL;
//     }
// }