// ✅ ShopItemUI.cs - 슬롯 클릭 시 패널 열기만 처리
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopItemUI : MonoBehaviour
{
    public TMP_Text titleText;
    public Image iconImage;
    public TMP_Text countText;
    public TMP_Text priceText;
    public TMP_Text progressText;
    public Image CurrencyIcon;

    private ShopOfferData currentOffer;

    public void SetOffer(ShopOfferData offer, PlayerCardInventory inventory,
        Dictionary<string, MonsterData> monsterDB, UpgradeRequirementDB upgradeDB)
    {
        if (offer == null || offer.item == null)
        {
            Debug.LogError("❌ offer 또는 offer.item 이 null입니다.");
            return;
        }

        if (!monsterDB.TryGetValue(offer.item.itemId, out var monsterData))
        {
            Debug.LogError($"❌ monsterDB에서 {offer.item.itemId} 를 찾지 못했습니다.");
            return;
        }

        if (offer.item.iconPath == null)
        {
            Debug.LogWarning($"⚠️ iconPath가 null입니다. 아이콘 리소스 로드에 실패했을 수 있음 → {offer.item.icon}");
        }

        currentOffer = offer;

        titleText.text = offer.item.title;
        iconImage.sprite = offer.item.iconPath;
        countText.text = "x" + offer.quantity;
        priceText.text = offer.price.ToString();
        CurrencyIcon.sprite = CurrencyIconManager.GetSprite(offer.currencyType);
        
        int owned = inventory.GetCardCount(offer.item.itemId);
        int level = inventory.GetCardLevel(offer.item.itemId);
        int required = upgradeDB.GetRequiredCards(monsterData.rarity, level);

        // 방어 로직 포함
        if (required <= 0 || required >= 1000000)
            progressText.text = "-";
        else
            progressText.text = $"{owned}/{required}";

        // 슬롯 클릭 시 패널 열기
        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(() =>
        {
            ShopItemPanelController.Instance.OpenPanel(offer, inventory, monsterDB, upgradeDB);
        });
    }
}

public static class CurrencyIconManager
{
    private static Dictionary<CurrencyType, Sprite> iconMap = new();

    public static void Initialize(Sprite gold, Sprite gem)
    {
        iconMap[CurrencyType.Gold] = gold;
        iconMap[CurrencyType.Gem] = gem;
    }

    public static Sprite GetSprite(CurrencyType type)
    {
        return iconMap.TryGetValue(type, out var sprite) ? sprite : null;
    }
}