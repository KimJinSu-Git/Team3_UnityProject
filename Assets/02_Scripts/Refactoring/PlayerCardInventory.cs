using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerCardInventory : MonoBehaviour
{
    public List<PlayerCardData> allOwnedCards = new();

    /// <summary>
    /// 카드 ID로 보유 카드 데이터를 가져옵니다.
    /// </summary>
    public PlayerCardData GetCardData(string id)
    {
        return allOwnedCards.FirstOrDefault(c => c.id == id);
    }

    /// <summary>
    /// 해당 카드 ID를 보유 중인지 확인합니다.
    /// </summary>
    public bool HasCard(string id)
    {
        return GetCardData(id) != null;
    }

    /// <summary>
    /// 카드 수량을 반환합니다.
    /// </summary>
    public int GetCardCount(string id)
    {
        return GetCardData(id)?.ownedCount ?? 0;
    }

    /// <summary>
    /// 카드 레벨을 반환합니다.
    /// </summary>
    public int GetCardLevel(string id)
    {
        return GetCardData(id)?.level ?? 1;
    }

    /// <summary>
    /// 카드 수량을 추가합니다. 신규 카드면 생성합니다.
    /// </summary>
    public void AddCard(string id, int amount, int startLevel = 1)
    {
        var card = GetCardData(id);
        if (card != null)
        {
            card.ownedCount += amount;
        }
        else
        {
            allOwnedCards.Add(new PlayerCardData(id, startLevel, amount));
        }
    }

    /// <summary>
    /// 카드 레벨을 증가시키고 수량 차감 (업그레이드 시 사용)
    /// </summary>
    public void UpgradeCard(string id, int cost)
    {
        var card = GetCardData(id);
        if (card != null && card.ownedCount >= cost)
        {
            card.ownedCount -= cost;
            card.level++;
        }
    }

    /// <summary>
    /// monsterData 연결 (런타임에서 한 번 호출)
    /// </summary>
    public void AttachMonsterData(Dictionary<string, MonsterData_Mainmenu> monsterDB)
    {
        foreach (var card in allOwnedCards)
        {
            if (monsterDB.TryGetValue(card.id, out var data))
                card.monsterData = data;
        }
    }
}