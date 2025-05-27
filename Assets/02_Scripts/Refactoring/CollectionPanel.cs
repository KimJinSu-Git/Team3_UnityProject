using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CollectionPanel : MonoBehaviour
{
    public Transform collectionGrid;
    public GameObject slotPrefab;

    public DeckManager_UI deckManager;
    [SerializeField] private UpgradeRequirementDB upgradeDB;
    [SerializeField] private PlayerCardInventory inventory;
    [SerializeField] private MonsterData_Mainmenu[] monsterList;

    private Dictionary<string, MonsterData_Mainmenu> monsterDB;
    private List<PlayerCardData> allCards => inventory.allOwnedCards;

    void Awake()
    {
        // Dictionary 초기화
        monsterDB = monsterList.ToDictionary(m => m.id, m => m);

        // monsterData 연결
        foreach (var card in allCards)
        {
            if (monsterDB.TryGetValue(card.id, out var data))
                card.monsterData = data;
        }
    }

    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        foreach (Transform child in collectionGrid)
            Destroy(child.gameObject);

        Debug.Log($"[CollectionPanel] 카드 개수: {allCards.Count}");
        foreach (var card in allCards)
        {
            if (card.monsterData == null)
            {
                Debug.LogWarning($"[CollectionPanel] monsterData가 연결되지 않음: {card.id}");
                continue;
            }

            if (deckManager.IsInDeck(card.id))
            {
                continue; // 이미 덱에 있는 카드는 스킵
            }

            var slot = Instantiate(slotPrefab, collectionGrid).GetComponent<SlotUI>();
            slot.Setup(card, -1, deckManager, SlotMode.Collection);
        }
    }

    public void ReturnCard(PlayerCardData card)
    {
        if (!allCards.Contains(card))
            allCards.Add(card);
    }

    public void RemoveCardFromCollection(PlayerCardData card)
    {
        allCards.Remove(card);
    }
}