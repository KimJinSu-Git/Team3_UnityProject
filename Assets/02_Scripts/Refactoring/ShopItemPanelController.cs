using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopItemPanelController : MonoBehaviour
{
    public static ShopItemPanelController Instance { get; private set; }

    [Header("UI References")]
    public TMP_Text titleText;
    public TMP_Text countText;
    public TMP_Text cardNameText;
    public TMP_Text rarityText;
    public Image iconImage;
    public Image currencyIcon;
    public Button buyButton;
    public Button closeButton;

    private ShopOfferData currentOffer;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        gameObject.SetActive(false);

        buyButton.onClick.AddListener(BuyItem);
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    /// <summary>
    /// 상점 아이템 상세 패널을 열고 UI 업데이트
    /// </summary>
    /// <param name="offer">해당 슬롯의 판매 데이터</param>
    /// <param name="baseDataDB">BaseData 딕셔너리</param>
    public void OpenPanel(ShopOfferData offer, Dictionary<string, BaseData> baseDataDB)
    {
        this.currentOffer = offer;

        // BaseData 연결
        if (baseDataDB != null && baseDataDB.TryGetValue(offer.item.itemId, out var baseData))
        {
            offer.item.baseData = baseData;
        }

        if (offer.item.baseData == null)
        {
            Debug.LogError($"[ShopPanel] ❌ BaseData가 없습니다: {offer.item.itemId}");
            return;
        }

        var bd = offer.item.baseData;

        // UI 업데이트
        titleText.text = bd.name;
        iconImage.sprite = bd.icon != null ? bd.icon : offer.item.iconPath;
        countText.text = $"x {offer.quantity}";
        cardNameText.text = bd.name;
        rarityText.text = bd.rarity.ToString();
        currencyIcon.sprite = CurrencyIconManager.GetSprite(offer.currencyType);
        buyButton.GetComponentInChildren<TMP_Text>().text = offer.price.ToString();

        gameObject.SetActive(true);
    }

    /// <summary>
    /// 구매 버튼 클릭 시 처리
    /// </summary>
    public void BuyItem()
    {
        if (!PlayerWallet.Instance.TrySpendCurrency(currentOffer.currencyType, currentOffer.price))
        {
            Debug.LogWarning("[ShopPanel] ❌ 재화 부족");
            return;
        }

        var bd = currentOffer.item.baseData;

        Debug.Log($"🛒 {bd.name} {currentOffer.quantity}개 구매 완료");

        // TODO: 인벤토리에 추가하는 처리 필요 시 여기에 삽입

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 외부에서 수동으로 패널만 보이게 할 때 사용
    /// </summary>
    public void ShowPanelOnly()
    {
        gameObject.SetActive(true);
    }
}
