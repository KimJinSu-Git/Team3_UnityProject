using UnityEngine;

[System.Serializable]
public class ShopItemData
{
    public string itemId;            // 예: "card_orc001"
    public string title;             // UI에 표시할 이름
    public string icon;              // JSON 내 아이콘 이름 (Resources 경로)
    [System.NonSerialized] public Sprite iconPath; // Resources.Load<Sprite>로 할당
    
    [System.NonSerialized] public BaseData baseData;

    // 카드 아이템 여부 확인
    public bool IsCardItem => itemId.StartsWith("card_");

    // 카드 아이템일 때 카드 ID만 추출 (접두사 "card_" 제거)
    public string ExtractCardId 
    {
        get
        {
            if (IsCardItem)
                return itemId.Substring("card_".Length);
            else
                return itemId;
        }
    }
}


[System.Serializable]
public class ShopOfferData
{
    public string offerId;           // 고유 오퍼 ID
    public ShopItemData item;        // 아이템 정보
    public int quantity;             // 구매 수량
    public int price;                // 가격
    public string currency;          // JSON에서 문자열로 받는 통화명 ("Gold", "Gem" 등)
    
    [System.NonSerialized] public CurrencyType currencyType; // 변환된 enum 값

    public bool isLimited;           // 제한 구매 여부 (ex. 하루 1회)
}