using UnityEngine;

[System.Serializable]
public class ShopItemData
{
    public string itemId;             // 예: "card_orc001"
    public string title;             // 아이템 이름 (UI용)
    public string icon;              // JSON에서 지정된 아이콘 경로명
    [System.NonSerialized] public Sprite iconPath; // Resources에서 Load 후 할당

    public bool IsCardItem => itemId.StartsWith("card_");
    public string ExtractCardId => itemId.Replace("card_", "");
}


[System.Serializable]
public class ShopOfferData
{
    public string offerId;
    public ShopItemData item;         // 실제 아이템 정보
    public int quantity;              // 구매 시 지급 수량
    public int price;                 // 가격
    public string currency; // ← JSON에서는 문자열로 들어오므로 이렇게 선언
    [System.NonSerialized] public CurrencyType currencyType; // ← 실제 enum값으로 사용
    public bool isLimited;            // 하루 1회 구매 제한 등
}