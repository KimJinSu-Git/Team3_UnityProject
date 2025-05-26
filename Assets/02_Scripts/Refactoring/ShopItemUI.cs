using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    public TMP_Text titleText;
    public Image iconImage;
    public TMP_Text countText;
    public Image progressFill;
    public TMP_Text priceText;
    public Image CurrencyIcon;
    public Button buyButton;

    private ShopOfferData currentOffer;

    
    public void SetOffer(ShopOfferData offer, PlayerCardInventory inventory,
        Dictionary<string, MonsterData_Mainmenu> monsterDB, UpgradeRequirementDB upgradeDB)
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

        if (monsterData == null)
        {
            Debug.LogError($"❌ monsterData 자체가 null입니다: {offer.item.itemId}");
            return;
        }

        if (offer.item.iconPath == null)
        {
            Debug.LogWarning($"⚠️ iconPath가 null입니다. 아이콘 리소스 로드에 실패했을 수 있음 → {offer.item.icon}");
        }

        // 정상 설정
        titleText.text = offer.item.title;
        iconImage.sprite = offer.item.iconPath;
        countText.text = "x" + offer.quantity;
        priceText.text = offer.price.ToString();
        CurrencyIcon.sprite = CurrencyIconManager.GetSprite(offer.currency);
    }



    private void OnClickBuy(PlayerCardInventory inventory, Dictionary<string, MonsterData_Mainmenu> monsterDB, UpgradeRequirementDB upgradeDB)
    {
        if (!currentOffer.item.IsCardItem) return;

        string cardId = currentOffer.item.ExtractCardId;

        if (!monsterDB.TryGetValue(cardId, out var monsterData))
        {
            Debug.LogError($"[ShopItemUI] 몬스터 데이터 로드 실패: {cardId}");
            return;
        }

        if (!PlayerWallet.Instance.TrySpendCurrency(currentOffer.currency, currentOffer.price))
        {
            Debug.LogWarning("[ShopItemUI] 재화 부족");
            return;
        }

        var card = inventory.GetCardData(cardId);
        if (card != null)
        {
            card.ownedCount += currentOffer.quantity;
        }
        else
        {
            int startLevel = upgradeDB.GetStartLevel(monsterData.rarity);
            inventory.allOwnedCards.Add(new PlayerCardData(cardId, startLevel, currentOffer.quantity)
            {
                monsterData = monsterData
            });
        }

        Debug.Log($"✅ {monsterData.monsterName} 카드 {currentOffer.quantity}장 구매 완료");

        // (선택) 버튼 비활성화 처리
        if (currentOffer.isLimited)
            buyButton.interactable = false;
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