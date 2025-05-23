using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class DeckManager_UI : MonoBehaviour
{
    [SerializeField] private Transform[] slotPositions; // 6칸짜리 Transform 배열
    
    public GameObject slotPrefab;
    public List<MonsterData_Mainmenu> currentDeck = new();
    public int maxDeckSize = 6;

    public CollectionPanel collectionPanel;
    
    public Dictionary<string, int> ownedCardDict = new();
    public UpgradeRequirementDB upgradeDB;

    public void TryAddCard(MonsterData_Mainmenu card)
    {
        if (currentDeck.Contains(card)) return;
        if (currentDeck.Count >= maxDeckSize) return;

        currentDeck.Add(card);
        RefreshDeckUI();
        collectionPanel.Refresh();
    }

    public void RemoveCard(int index)
    {
        if (index < 0 || index >= currentDeck.Count)
        {
            Debug.LogWarning($"[DeckManager] ❌ 잘못된 인덱스 접근 시도: {index}");
            return;
        }

        Debug.Log($"[DeckManager] ✅ RemoveCard 실행: index = {index}");

        currentDeck.RemoveAt(index);
        RefreshDeckUI();
        collectionPanel.Refresh();
    }



    public void RefreshDeckUI()
    {
        // ✅ 기존 슬롯 제거
        foreach (var pos in slotPositions)
        {
            foreach (Transform child in pos)
                Destroy(child.gameObject);
        }

        // ✅ 현재 덱 기준으로 슬롯 생성
        for (int i = 0; i < slotPositions.Length; i++)
        {
            if (i >= currentDeck.Count) break;

            GameObject slotGO = Instantiate(slotPrefab);
            var slot = slotGO.GetComponent<SlotUI>();
            slot.Setup(currentDeck[i], i, this, SlotMode.Deck);
            slotGO.transform.SetParent(slotPositions[i], false);
        }

    }

    public bool IsInDeck(string id) => currentDeck.Any(c => c.id == id);
}