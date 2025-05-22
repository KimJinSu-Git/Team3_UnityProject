using System.Collections.Generic;
using UnityEngine;

public class CollectionPanel : MonoBehaviour
{
    [Header("UI 구성")]
    [SerializeField] private Transform collectionGrid;
    [SerializeField] private GameObject slotPrefab;

    [Header("데이터")]
    [SerializeField] private List<MonsterData_Mainmenu> allCards;
    [SerializeField] private PlayerCardInventory playerInventory;
    [SerializeField] private DeckManager_UI deckManager;

    public void Refresh()
    {
        // 기존 슬롯 삭제
        foreach (Transform child in collectionGrid)
            Destroy(child.gameObject);

        foreach (var card in allCards)
        {
            // 덱에 있는 카드는 컬렉션에서 제외
            if (deckManager.IsInDeck(card.id)) continue;

            GameObject slotGO = Instantiate(slotPrefab, collectionGrid);
            var slot = slotGO.GetComponent<SlotUI>();

            bool isOwned = playerInventory.HasCard(card.id);
            slot.Init(card, SlotUI.SlotMode.Collection, isOwned);
        }
    }
}