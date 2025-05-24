using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RarityLevelInfo
{
    public MonsterData_Mainmenu.Rarity rarity;

    [Tooltip("레벨 1~10에서 다음 레벨로 가기 위한 카드 수")]
    public List<int> cardsNeededPerLevel = new List<int>(new int[10]);

    [Tooltip("레벨 1~10에서 다음 레벨로 가기 위한 골드량")]
    public List<int> goldNeededPerLevel = new List<int>(new int[10]);
}

public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private UpgradeRequirementDB upgradeDB;

    // 보유 정보 (게임에 따라 PlayerData 등에서 가져올 수도 있음)
    public int GetOwnedCardCount(string monsterId)
    {
        // 예: 실제 구현은 데이터 매니저에서 가져오기
        return 25;
    }

    public int GetPlayerGold()
    {
        return 1000; // 예: 실제 구현은 플레이어 재화 시스템에서 가져오기
    }

    public bool TryUpgrade(MonsterData_Mainmenu monster)
    {
        if (!upgradeDB.CanUpgrade(monster))
        {
            Debug.Log("최대 레벨입니다.");
            return false;
        }

        int requiredCards = upgradeDB.GetRequiredCards(monster.rarity, monster.level);
        int requiredGold = upgradeDB.GetRequiredGold(monster.rarity, monster.level);

        int ownedCards = GetOwnedCardCount(monster.id);
        int currentGold = GetPlayerGold();

        if (ownedCards < requiredCards)
        {
            Debug.Log("카드 수 부족");
            return false;
        }

        if (currentGold < requiredGold)
        {
            Debug.Log("골드 부족");
            return false;
        }

        // 재화 차감 & 레벨업 처리
        monster.level += 1;
        Debug.Log($"레벨업 성공! → {monster.monsterName} : 레벨 {monster.level}");
        return true;
    }
}