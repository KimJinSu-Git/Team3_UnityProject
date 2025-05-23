using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OwnedCardData
{
    public string cardId;
    public int level;
    public int count;
}

public class PlayerCardInventory : MonoBehaviour
{
    public List<OwnedCardData> ownedCards = new();

    public bool HasCard(string id) => ownedCards.Exists(c => c.cardId == id);
    public int GetCardCount(string id) => ownedCards.Find(c => c.cardId == id)?.count ?? 0;
    public int GetCardLevel(string id) => ownedCards.Find(c => c.cardId == id)?.level ?? 1;
}

