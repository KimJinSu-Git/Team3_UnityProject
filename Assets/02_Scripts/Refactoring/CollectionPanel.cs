using System.Collections.Generic;
using UnityEngine;

public class CollectionPanel : MonoBehaviour
{
    public Transform collectionGrid;
    public GameObject slotPrefab;
    public List<PlayerCardData> allCards;
    public PlayerCardInventory inventory;
    public DeckManager_UI deckManager;

    public void Start()
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
                Debug.LogWarning("[CollectionPanel] monsterData가 연결되지 않음");
                continue;
            }

            Debug.Log($"[CollectionPanel] 생성 시도: {card.monsterData.monsterName}");

            if (deckManager.IsInDeck(card.id))
            {
                Debug.Log($"[CollectionPanel] 이미 덱에 포함된 카드: {card.monsterData.monsterName}");
                continue;
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