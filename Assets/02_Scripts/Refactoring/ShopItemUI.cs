using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Image currencyIcon;
    [SerializeField] private Button slotButton; // 슬롯 전체 클릭용 버튼 (없으면 buyButton만 써도 됨)

    private ShopOfferData offer;
    private Dictionary<string, BaseData> baseDataDB;

    public void SetOffer(ShopOfferData offer, Dictionary<string, BaseData> baseDataDB)
    {
        this.offer = offer;
        this.baseDataDB = baseDataDB; // 저장!

        if (baseDataDB != null && baseDataDB.TryGetValue(offer.item.itemId, out var baseData))
        {
            offer.item.baseData = baseData;
            nameText.text = SlotUI.koreanNameMap.TryGetValue(baseData.name, out var korName) ? korName : baseData.name;
            iconImage.sprite = baseData.icon != null ? baseData.icon : offer.item.iconPath;
        }
        else
        {
            nameText.text = offer.item.title ?? offer.item.itemId;
            iconImage.sprite = offer.item.iconPath;
        }

        priceText.text = offer.price.ToString();
        currencyIcon.sprite = CurrencyIconManager.GetSprite(offer.currencyType);
    }

    public void OnSlotClicked()
    {
        if (ShopItemPanelController.Instance != null && offer != null)
        {
            ShopItemPanelController.Instance.OpenPanel(offer, baseDataDB); // ← 인수 맞춰서 호출
        }
    }
}