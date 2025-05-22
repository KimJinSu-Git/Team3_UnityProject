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
    public TMP_Text labelText;
    public Image refreshButtonImage;

    [Header("설정")]
    public int displayCount = 6;
    public GameObject shopItemPrefab;
    public Transform shopParent;
    public float cooldownTime = 5f;

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

        refreshCount = PlayerPrefs.GetInt("refreshCount", 0); // 🟡 저장된 리프레시 카운트 불러오기

        if (PlayerWallet.Instance != null)
        {
            PlayerWallet.Instance.AddGold(10000); // 테스트용
            PlayerWallet.Instance.AddGem(1000); // 빌드 시 삭제해야함.
        }

        ForceRefreshOnEnter();
        UpdateCostUI();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace)) // 테스트용 빌드 시 삭제해야함.
        {
            refreshCount = 0;
            PlayerPrefs.DeleteKey("refreshCount");
            UpdateCostUI();
            Debug.Log("🔄 리프레시 카운트 초기화됨");
        }
    }

    private void ForceRefreshOnEnter()
    {
        // 기존 슬롯 제거
        foreach (Transform child in shopParent)
            Destroy(child.gameObject);

        currentOffers = allOffers.OrderBy(x => Random.value).Take(displayCount).ToList();

        foreach (var offer in currentOffers)
        {
            var slot = Instantiate(shopItemPrefab, shopParent);
            slot.GetComponent<ShopItemUI>().SetOffer(offer);
        }

        Debug.Log("[Shop] ✅ 첫 진입 시 상점 초기화 완료");
    }


    void LoadAllOffers()
    {
        var json = Resources.Load<TextAsset>("shop_offers");

        if (json == null)
        {
            Debug.LogError("[Shop] shop_offers.json 로드 실패!");
            return;
        }

        Debug.Log("[Shop] JSON 내용: " + json.text.Substring(0, Mathf.Min(json.text.Length, 200)));

        var wrapper = JsonUtility.FromJson<ShopOfferListWrapper>(json.text);
        allOffers = wrapper.offers;

        if (allOffers == null || allOffers.Count == 0)
        {
            Debug.LogError("[Shop] offers 로드 실패 or 비어 있음!");
        }
        else
        {
            Debug.Log($"[Shop] 총 오퍼 수: {allOffers.Count}");
        }

        foreach (var offer in allOffers)
        {
            offer.item.iconPath = Resources.Load<Sprite>($"Icons/{offer.item.icon}");
        }
    }


    public void RefreshShop()
    {
        Debug.Log("[Shop] 🔁 RefreshShop 호출됨");

        if (isCoolingDown)
        {
            Debug.LogWarning("[Shop] ❌ 쿨타임 중이라 리프레시 불가");
            return;
        }

        CurrencyType currency;
        int cost = GetRefreshCost(out currency);
        Debug.Log($"[Shop] 현재 비용: {cost}, 통화: {currency}");

        // 통화 차감
        if (currency == CurrencyType.Gold)
        {
            if (!PlayerWallet.Instance.HasEnoughGold(cost))
            {
                Debug.LogWarning("[Shop] ❌ 골드 부족");
                return;
            }
            PlayerWallet.Instance.SpendGold(cost);
        }
        else
        {
            if (!PlayerWallet.Instance.HasEnoughGem(cost))
            {
                Debug.LogWarning("[Shop] ❌ 젬 부족");
                return;
            }
            PlayerWallet.Instance.SpendGem(cost);
        }
        
        refreshCount++;
        PlayerPrefs.SetInt("refreshCount", refreshCount);

        Debug.Log("[Shop] ✅ 비용 차감 완료, 리프레시 시작");
        Debug.Log($"[Shop] Refresh Count = {refreshCount}");
        
        // 슬롯 제거 및 생성
        foreach (Transform child in shopParent)
            Destroy(child.gameObject);

        currentOffers = allOffers.OrderBy(x => Random.value).Take(displayCount).ToList();

        foreach (var offer in currentOffers)
        {
            var slot = Instantiate(shopItemPrefab, shopParent);
            slot.GetComponent<ShopItemUI>().SetOffer(offer);
        }

        UpdateCostUI();
        StartCoroutine(CooldownCoroutine());
    }

    int GetRefreshCost(out CurrencyType currency)
    {
        if (refreshCount < 10)
        {
            currency = CurrencyType.Gold;
            return 100 + 50 * refreshCount;
        }
        else
        {
            currency = CurrencyType.Gem;
            return 50 + 25 * (refreshCount - 10);
        }
    }

    void UpdateCostUI()
    {
        CurrencyType currency;
        int cost = GetRefreshCost(out currency);
        costText.text = cost.ToString();
        currencyIcon.sprite = CurrencyIconManager.GetSprite(currency);
    }
    
    IEnumerator CooldownCoroutine()
    {
        isCoolingDown = true;
        refreshButtonImage.color = Color.gray;
        currencyIcon.color = Color.gray;
        
        float timeLeft = cooldownTime;
        while (timeLeft > 0)
        {
            labelText.text = $"{Mathf.CeilToInt(timeLeft)}초";
            timeLeft -= Time.deltaTime;
            yield return null;
        }

        labelText.text = "새로고침";

        refreshButtonImage.color = Color.white;
        currencyIcon.color = Color.white;

        isCoolingDown = false;
    }
}
