using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class DeckManager_UI : MonoBehaviour
{
    [SerializeField] private Transform[] slotPositions;
    public GameObject slotPrefab;
    public List<PlayerCardData> currentDeck = new();
    public int maxDeckSize = 6;

    public CollectionPanel collectionPanel;
    public UpgradeRequirementDB upgradeDB;

    public bool isReplaceMode;
    public PlayerCardData replaceTargetCard;

    public void TryAddCard(PlayerCardData card)
    {
        if (currentDeck.Any(c => c.id == card.id)) return;
        if (currentDeck.Count >= maxDeckSize) return;

        currentDeck.Add(card);
        RefreshDeckUI();
        collectionPanel.Refresh();
    }

    public void RemoveCard(int index)
    {
        if (index < 0 || index >= currentDeck.Count) return;

        var removedCard = currentDeck[index];
        currentDeck.RemoveAt(index);

        collectionPanel.ReturnCard(removedCard);

        RefreshDeckUI();
        collectionPanel.Refresh();
    }

    public void RefreshDeckUI()
    {
        foreach (var pos in slotPositions)
        {
            foreach (Transform child in pos)
                Destroy(child.gameObject);
        }

        for (int i = 0; i < currentDeck.Count; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, slotPositions[i], false);
            var slot = slotGO.GetComponent<SlotUI>();
            slot.Setup(currentDeck[i], i, this, SlotMode.Deck);
        }
    }

    public bool IsInDeck(string id) => currentDeck.Any(c => c.id == id);

    public void TryAddOrReplace(PlayerCardData card)
    {
        if (IsInDeck(card.id)) return;

        if (currentDeck.Count < maxDeckSize)
        {
            currentDeck.Add(card);
            RefreshDeckUI();
            collectionPanel.Refresh();
        }
        else
        {
            isReplaceMode = true;
            replaceTargetCard = card;
            Debug.Log($"[DeckManager] 교체 모드 진입: {card.monsterData.monsterName}");

            RefreshDeckUI();
        }
    }

    public void ReplaceCard(int index)
    {
        if (!isReplaceMode || replaceTargetCard == null) return;

        var oldCard = currentDeck[index];
        currentDeck[index] = replaceTargetCard;

        collectionPanel.ReturnCard(oldCard);
        collectionPanel.RemoveCardFromCollection(replaceTargetCard);

        isReplaceMode = false;
        replaceTargetCard = null;

        RefreshDeckUI();
        collectionPanel.Refresh();
    }
}