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
    public List<OwnedCardData> ownedCards;

    public bool HasCard(string id)
    {
        return ownedCards.Exists(c => c.cardId == id);
    }

    public int GetCardCount(string id)
    {
        var card = ownedCards.Find(c => c.cardId == id);
        return card != null ? card.count : 0;
    }

    public int GetCardLevel(string id)
    {
        var card = ownedCards.Find(c => c.cardId == id);
        return card != null ? card.level : 1;
    }
}

