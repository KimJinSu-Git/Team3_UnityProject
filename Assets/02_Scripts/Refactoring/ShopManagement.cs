// ✅ ShopManagement (상점 전체 관리)
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
    
    [Header("레퍼런스")]
    public PlayerCardInventory inventory;
    public UpgradeRequirementDB upgradeDB;
    
    [Header("몬스터 리스트")]
    [SerializeField] private MonsterData_Mainmenu[] monsterList;
    public Dictionary<string, MonsterData_Mainmenu> monsterDB { get; private set; }
    
    void Awake()
    {
        // 💡 먼저 싱글톤 지정
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // ✅ 그다음 Dictionary 초기화
        monsterDB = monsterList.ToDictionary(m => m.id, m => m);

        if (monsterDB.Count == 0)
            Debug.LogError("❌ monsterDB 초기화 실패 - monsterList 비어있음");
        else
            Debug.Log($"✅ monsterDB 초기화 완료: {monsterDB.Count}개");
    }


    void Start()
    {
        Debug.Log($"[Shop] inventory: {(inventory == null ? "❌ null" : "✅ ok")}");
        Debug.Log($"[Shop] monsterDB: {(monsterDB == null ? "❌ null" : "✅ ok")}");
        Debug.Log($"[Shop] upgradeDB: {(upgradeDB == null ? "❌ null" : "✅ ok")}");
        
        LoadAllOffers();

        refreshCount = PlayerPrefs.GetInt("refreshCount", 0);

        if (PlayerWallet.Instance != null)
        {
            PlayerWallet.Instance.AddCurrency(CurrencyType.Gold, 10000); // 테스트용
            PlayerWallet.Instance.AddCurrency(CurrencyType.Gem, 1000); // 빌드 시 삭제
        }

        ForceRefreshOnEnter();
        UpdateCostUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace)) // 테스트용 초기화
        {
            refreshCount = 0;
            PlayerPrefs.DeleteKey("refreshCount");
            UpdateCostUI();
            Debug.Log("🔄 리프레시 카운트 초기화됨");
        }
    }

    private void ForceRefreshOnEnter()
    {
        ClearShopItems();
        currentOffers = GetRandomOffers();
        DisplayOffers(currentOffers);
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

        if (!PlayerWallet.Instance.TrySpendCurrency(currency, cost))
        {
            Debug.LogWarning("[Shop] ❌ 재화 부족");
            return;
        }

        refreshCount++;
        PlayerPrefs.SetInt("refreshCount", refreshCount);

        Debug.Log("[Shop] ✅ 비용 차감 완료, 리프레시 시작");
        Debug.Log($"[Shop] Refresh Count = {refreshCount}");

        ClearShopItems();
        currentOffers = GetRandomOffers();
        DisplayOffers(currentOffers);

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

    void ClearShopItems()
    {
        foreach (Transform child in shopParent)
            Destroy(child.gameObject);
    }

    List<ShopOfferData> GetRandomOffers()
    {
        return allOffers.OrderBy(x => Random.value).Take(displayCount).ToList();
    }

    // DisplayOffers 수정
    void DisplayOffers(List<ShopOfferData> offers)
    {
        foreach (var offer in offers)
        {
            var slot = Instantiate(shopItemPrefab, shopParent);
            slot.GetComponent<ShopItemUI>().SetOffer(
                offer,
                inventory,
                monsterDB,
                upgradeDB
            );
        }
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