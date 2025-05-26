using System.Collections.Generic;
using UnityEngine;

public class CollectionPanel : MonoBehaviour
{
    public Transform collectionGrid;
    public GameObject slotPrefab;
    public List<MonsterData_Mainmenu> allCards;
    public PlayerCardInventory inventory;
    public DeckManager_UI deckManager;

    public void Start()
    { 
        Refresh(); // ← 이거 빠지면 아무 슬롯도 생성 안 됨
    }
    
    public void Refresh()
    {
        foreach (Transform child in collectionGrid)
            Destroy(child.gameObject);

        Debug.Log($"[CollectionPanel] 카드 개수: {allCards.Count}");
        foreach (var card in allCards)
        {
            Debug.Log($"[CollectionPanel] 생성 시도: {card.monsterName}");

            if (deckManager.IsInDeck(card.id))
            {
                Debug.Log($"[CollectionPanel] 이미 덱에 포함된 카드: {card.monsterName}");
                continue;
            }

            var slot = Instantiate(slotPrefab, collectionGrid).GetComponent<SlotUI>();
            slot.Setup(card, -1, deckManager, SlotMode.Collection);
        }
    }
}