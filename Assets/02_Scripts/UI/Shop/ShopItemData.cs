using UnityEngine;

[System.Serializable]
public class ShopItemData
{
    public string itemId;
    public string title;
    public string icon;  // ← JSON에서는 이걸 채움
    [System.NonSerialized] public Sprite iconPath; // ← 코드에서 Load 후 대입
}


[System.Serializable]
public class ShopOfferData
{
    public string offerId;
    public ShopItemData item;     // 참조 or itemId로 매핑
    public int quantity;
    public int price;
    public CurrencyType currency;
    public bool isLimited;        // 하루 1회 구매 제한 등
}

public enum CurrencyType
{
    Gold,
    Gem,
    Elixir // 인게임 전용
}
