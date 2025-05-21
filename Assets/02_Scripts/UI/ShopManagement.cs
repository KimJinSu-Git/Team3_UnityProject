using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class ShopOfferListWrapper
{
    public List<ShopOfferData> offers;
}

public class ShopManagement : MonoBehaviour
{
    public static ShopManagement Instance { get; private set; }

    [Header("Refresh")]
    public TMP_Text costText;
    public Image currencyIcon;

    [Header("설정")]
    public int displayCount = 6;
    public GameObject shopItemPrefab;
    public Transform shopParent;
    public float cooldownTime = 10f;

    private List<ShopOfferData> allOffers;
    private List<ShopOfferData> currentOffers;

    private int refreshCount = 0;
    private bool isCoolingDown = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        LoadAllOffers();
        RefreshShop();
        UpdateCostUI();
    }

    void LoadAllOffers()
    {
        var json = Resources.Load<TextAsset>("shop_offers");
        var wrapper = JsonUtility.FromJson<ShopOfferListWrapper>(json.text);
        allOffers = wrapper.offers;

        foreach (var offer in allOffers)
        {
            offer.item.iconPath = Resources.Load<Sprite>($"Icons/{offer.item.icon}");
        }
    }

    public void RefreshShop()
    {
        if (isCoolingDown) return;

        CurrencyType currency;
        int cost = GetRefreshCost(out currency);

        // 통화 차감
        if (currency == CurrencyType.Gold)
        {
            if (!PlayerWallet.Instance.HasEnoughGold(cost)) return;
            PlayerWallet.Instance.SpendGold(cost);
        }
        else
        {
            if (!PlayerWallet.Instance.HasEnoughGem(cost)) return;
            PlayerWallet.Instance.SpendGem(cost);
        }

        refreshCount++;

        // 기존 슬롯 제거
        foreach (Transform child in shopParent)
            Destroy(child.gameObject);

        // 새로운 오퍼 랜덤 선택
        currentOffers = allOffers.OrderBy(x => Random.value).Take(displayCount).ToList();

        foreach (var offer in currentOffers)
        {
            var slot = Instantiate(shopItemPrefab, shopParent);
            slot.GetComponent<ShopItemUI>().SetOffer(offer);
        }

        UpdateCostUI();
        StartCoroutine(CooldownCoroutine());
    }

    IEnumerator CooldownCoroutine()
    {
        isCoolingDown = true;
        currencyIcon.color = new Color(1, 1, 1, 0.5f); // 반투명 처리

        yield return new WaitForSeconds(cooldownTime);

        isCoolingDown = false;
        currencyIcon.color = Color.white;
    }

    int GetRefreshCost(out CurrencyType currency)
    {
        if (refreshCount < 5)
        {
            currency = CurrencyType.Gold;
            return 100 + 50 * refreshCount;
        }
        else
        {
            currency = CurrencyType.Gem;
            return 50 + 25 * (refreshCount - 5);
        }
    }

    void UpdateCostUI()
    {
        CurrencyType currency;
        int cost = GetRefreshCost(out currency);
        costText.text = cost.ToString();
        currencyIcon.sprite = CurrencyIconManager.GetSprite(currency);
    }
}
