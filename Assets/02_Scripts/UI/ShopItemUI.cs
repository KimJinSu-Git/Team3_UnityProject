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

    public void SetOffer(ShopOfferData offer)
    {
        titleText.text = offer.item.title;
        iconImage.sprite = offer.item.iconPath;
        countText.text = "x" + offer.quantity;
        priceText.text = offer.price.ToString();
        CurrencyIcon.sprite = CurrencyIconManager.GetSprite(offer.currency);
    }
}

public static class CurrencyIconManager
{
    private static Dictionary<CurrencyType, Sprite> iconMap = new();

    public static void Initialize(Sprite gold, Sprite gem, Sprite elixir)
    {
        iconMap[CurrencyType.Gold] = gold;
        iconMap[CurrencyType.Gem] = gem;
        iconMap[CurrencyType.Elixir] = elixir;
    }

    public static Sprite GetSprite(CurrencyType type)
    {
        return iconMap.TryGetValue(type, out var sprite) ? sprite : null;
    }
}

