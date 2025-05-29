// using UnityEngine;
//
// public class UpgradeManager : MonoBehaviour
// {
//     [SerializeField] private UpgradeRequirementDB upgradeDB;
//
//     public bool TryUpgrade(PlayerCardData card)
//     {
//         if (!upgradeDB.CanUpgrade(card.level))
//         {
//             Debug.Log("최대 레벨입니다.");
//             return false;
//         }
//
//         if (card.monsterData == null)
//         {
//             Debug.LogWarning("monsterData가 연결되지 않았습니다.");
//             return false;
//         }
//
//         int requiredCards = upgradeDB.GetRequiredCards(card.monsterData.rarity, card.level);
//         int requiredGold = upgradeDB.GetRequiredGold(card.monsterData.rarity, card.level);
//
//         if (card.ownedCount < requiredCards)
//         {
//             Debug.Log("카드 수 부족");
//             return false;
//         }
//
//         if (!PlayerWallet.Instance.TrySpendCurrency(CurrencyType.Gold, requiredGold))
//         {
//             Debug.Log("골드 부족");
//             return false;
//         }
//
//         // 업그레이드 성공
//         card.ownedCount -= requiredCards;
//         card.level += 1;
//
//         Debug.Log($"[업그레이드 완료] {card.monsterData.name} → Lv.{card.level}");
//         return true;
//     }
// }