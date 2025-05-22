using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class DeckManager_UI : MonoBehaviour
{
    public const int MaxDeckSize = 6;

    [Header("덱 UI")]
    [SerializeField] private Transform deckGrid;
    [SerializeField] private GameObject deckSlotPrefab;
    [SerializeField] private TMP_Text averageCostText;

    [Header("외부 참조")]
    [SerializeField] private CollectionPanel collectionPanel;
    [SerializeField] public UpgradeRequirementDB upgradeDB;

    [Header("플레이어 카드 보유 정보")]
    public Dictionary<string, int> ownedCardDict = new(); // 카드 ID → 보유 수량

    public List<MonsterData_Mainmenu> currentDeck = new(); // 최대 6칸

    private void Start()
    {
        // 초기화 시 빈칸으로 채우기
        while (currentDeck.Count < MaxDeckSize)
            currentDeck.Add(null);

        RefreshDeckUI();
    }

    /// <summary> 카드가 덱에 포함되어 있는지 </summary>
    public bool IsInDeck(string cardId)
    {
        return currentDeck.Any(c => c != null && c.id == cardId);
    }

    /// <summary> 덱에 카드를 추가 (빈칸 우선) </summary>
    public void TryAddCard(MonsterData_Mainmenu card)
    {
        if (IsInDeck(card.id)) return;

        int emptyIndex = currentDeck.FindIndex(c => c == null);
        if (emptyIndex != -1)
        {
            currentDeck[emptyIndex] = card;
            RefreshDeckUI();
            collectionPanel.Refresh();
        }
    }

    /// <summary> 카드 교체용 - 인덱스에 새 카드 덮어쓰기 </summary>
    public void ReplaceCard(int index, MonsterData_Mainmenu newCard)
    {
        if (index >= 0 && index < MaxDeckSize)
        {
            currentDeck[index] = newCard;
            RefreshDeckUI();
            collectionPanel.Refresh();
        }
    }

    /// <summary> 덱 슬롯에서 제거 </summary>
    public void RemoveCard(int index)
    {
        if (index >= 0 && index < MaxDeckSize)
        {
            currentDeck[index] = null;
            RefreshDeckUI();
            collectionPanel.Refresh();
        }
    }

    /// <summary> 덱 UI 슬롯 전부 새로고침 </summary>
    public void RefreshDeckUI()
    {
        foreach (Transform child in deckGrid)
            Destroy(child.gameObject);

        float totalCost = 0f;

        for (int i = 0; i < MaxDeckSize; i++)
        {
            var card = currentDeck[i];
            var go = Instantiate(deckSlotPrefab, deckGrid);
            var slot = go.GetComponent<DeckSlotUI>();

            if (card != null)
            {
                int owned = ownedCardDict.ContainsKey(card.id) ? ownedCardDict[card.id] : 0;
                int required = upgradeDB.GetRequiredCards(card.rarity, card.level);
                slot.Init(card, i, this); // 덱 인덱스도 넘김
            }
            else
            {
                slot.Init(null, i, this); // 빈칸
            }

            if (card != null) totalCost += card.cost;
        }

        float avg = currentDeck.Count(c => c != null) > 0
            ? totalCost / currentDeck.Count(c => c != null)
            : 0;

        averageCostText.text = avg.ToString("0.0");
    }
}
