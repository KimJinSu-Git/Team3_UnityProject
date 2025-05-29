using System;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public UpgradeRequirementDB upgradeRequirementDB;  // 업그레이드 조건 DB

    // 카드가 업그레이드 가능한지 확인
    public bool CanUpgrade(BaseData cardData)
    {
        if (cardData.cardDataType != CardDataType.Monster)
            return false;

        MonsterData monster = cardData as MonsterData;
        if (monster == null)
            return false;

        if (!upgradeRequirementDB.CanUpgrade(monster.level))
            return false;

        int requiredCards = upgradeRequirementDB.GetRequiredCards(monster.rarity, monster.level);
        int requiredGold = upgradeRequirementDB.GetRequiredGold(monster.rarity, monster.level);

        // 소유 카드 수와 골드 체크 (PlayerWallet 사용)
        return monster.ownedCardCount >= requiredCards &&
               PlayerWallet.Instance.HasEnoughCurrency(CurrencyType.Gold, requiredGold);
    }

    // 카드 업그레이드 처리
    public bool UpgradeCard(BaseData cardData)
    {
        if (!CanUpgrade(cardData))
        {
            Debug.LogWarning("업그레이드 조건을 만족하지 못함.");
            return false;
        }

        MonsterData monster = cardData as MonsterData;
        int requiredCards = upgradeRequirementDB.GetRequiredCards(monster.rarity, monster.level);
        int requiredGold = upgradeRequirementDB.GetRequiredGold(monster.rarity, monster.level);

        // 재화 차감 시도
        bool spent = PlayerWallet.Instance.TrySpendCurrency(CurrencyType.Gold, requiredGold);
        if (!spent)
        {
            Debug.LogWarning("골드 차감 실패");
            return false;
        }

        // 카드 소모 및 레벨업
        monster.ownedCardCount -= requiredCards;
        monster.level++;

        ApplyLevelUpStats(monster);

        Debug.Log($"{monster.cardName}가 레벨 {monster.level}로 업그레이드 되었습니다!");

        // 덱 저장
        DeckManager.Instance.DeckSave();

        return true;
    }

    // 레벨업 시 스탯 보정 (예시)
    private void ApplyLevelUpStats(MonsterData monster)
    {
        monster.maxHP = Mathf.RoundToInt(monster.baseHP * (1 + 0.1f * (monster.level - 1)));
        monster.damage = Mathf.RoundToInt(monster.baseDamage * (1 + 0.1f * (monster.level - 1)));
        monster.attackSpeed = monster.baseAttackSpeed * (1 + 0.05f * (monster.level - 1));
        monster.moveSpeed = monster.baseMoveSpeed * (1 + 0.02f * (monster.level - 1));
    }
}
