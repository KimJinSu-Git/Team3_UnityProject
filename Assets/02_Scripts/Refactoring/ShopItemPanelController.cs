// 📦 ShopItemPanelController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopItemPanelController : MonoBehaviour
{
    public static ShopItemPanelController Instance { get; private set; }

    [Header("UI References")]
    public TMP_Text titleText;
    public TMP_Text progressText;
    public TMP_Text countText;
    public TMP_Text cardNameText;
    public TMP_Text rarityText;
    public Image iconImage;
    public Image currencyIcon;
    public Button buyButton;
    public Button closeButton;

    private ShopOfferData currentOffer;
    private PlayerCardInventory inventory;
    private Dictionary<string, MonsterData_Mainmenu> monsterDB;
    private UpgradeRequirementDB upgradeDB;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        gameObject.SetActive(false);
        buyButton.onClick.AddListener(BuyItem);
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    public void OpenPanel(ShopOfferData offer,
                          PlayerCardInventory inventory,
                          Dictionary<string, MonsterData_Mainmenu> monsterDB,
                          UpgradeRequirementDB upgradeDB)
    {
        this.currentOffer = offer;
        this.inventory = inventory;
        this.monsterDB = monsterDB;
        this.upgradeDB = upgradeDB;

        string cardId = offer.item.itemId;

        if (!monsterDB.TryGetValue(cardId, out var monster))
        {
            Debug.LogError($"[ShopPanel] 몬스터 데이터 없음: {cardId}");
            return;
        }

        // UI 채우기
        titleText.text = monster.monsterName;
        iconImage.sprite = offer.item.iconPath;
        countText.text = $"x {offer.quantity}";
        cardNameText.text = monster.monsterName;
        rarityText.text = monster.rarity.ToString();
        currencyIcon.sprite = CurrencyIconManager.GetSprite(offer.currencyType);
        buyButton.GetComponentInChildren<TMP_Text>().text = offer.price.ToString();

        int owned = inventory.GetCardCount(cardId);
        int level = inventory.GetCardLevel(cardId);
        int required = upgradeDB.GetRequiredCards(monster.rarity, level);
        progressText.text = $"{owned}/{required}";

        gameObject.SetActive(true);
    }

    public void BuyItem()
    {
        string cardId = currentOffer.item.itemId;

        if (!monsterDB.TryGetValue(cardId, out var monster))
        {
            Debug.LogError($"[ShopPanel] 몬스터 데이터 없음: {cardId}");
            return;
        }

        if (!PlayerWallet.Instance.TrySpendCurrency(currentOffer.currencyType, currentOffer.price))
        {
            Debug.LogWarning("[ShopPanel] ❌ 재화 부족");
            return;
        }

        var card = inventory.GetCardData(cardId);
        if (card != null)
        {
            card.ownedCount += currentOffer.quantity;
        }
        else
        {
            int startLevel = upgradeDB.GetStartLevel(monster.rarity);
            inventory.allOwnedCards.Add(new PlayerCardData(cardId, startLevel, currentOffer.quantity)
            {
                monsterData = monster
            });
        }

        Debug.Log($"🛒 {monster.monsterName} {currentOffer.quantity}장 구매 완료");
        gameObject.SetActive(false);
    }

    public void ShowPanelOnly()
    {
        gameObject.SetActive(true);
    }
}
