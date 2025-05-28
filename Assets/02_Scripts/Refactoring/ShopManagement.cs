// // ✅ ShopManagement (상점 전체 관리)
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
// using TMPro;
// using UnityEngine.UI;
//
// // JSON 복사용 파일을 목록으로 포인팅하기 위해 사용
// [System.Serializable]
// public class ShopOfferListWrapper
// {
//     public List<ShopOfferData> offers;
// }
//
// public class ShopManagement : MonoBehaviour
// {
//     public static ShopManagement Instance { get; private set; }
//
//     [Header("Refresh")]
//     public TMP_Text costText;                // 상점 갱신 비용 표시 텍스트
//     public Image currencyIcon;              // 결세 통화 이미지
//     public TMP_Text labelText;              // 갱신 버튼 리버트 텍스트
//     public Image refreshButtonImage;        // 갱신 버튼 이미지
//
//     [Header("설정")]
//     public int displayCount = 6;            // 표시할 상점 항목 개수
//     public GameObject shopItemPrefab;       // 상점 아이템 프리파브
//     public Transform shopParent;            // 상점 아이템 부모
//     public float cooldownTime = 5f;         // 자동 갱신 포탈 시간
//
//     private List<ShopOfferData> allOffers;          // 모든 판매 목록
//     private List<ShopOfferData> currentOffers;      // 현재 표시 중인 상점 항목
//
//     private int refreshCount = 0;                  // 갱신 횟수
//     private bool isCoolingDown = false;           // 자동 갱신 중인지 체크
//
//     [Header("레퍼런스")]
//     public PlayerCardInventory inventory;               // 플레이어 카드 인벤토리
//     //public UpgradeRequirementDB upgradeDB;             // 업그레이드 범위 DB
//
//     [Header("모델 목록")]
//     [SerializeField] private MonsterData[] monsterList;  // 사용할 모델 목록
//     public Dictionary<string, MonsterData> monsterDB { get; private set; } // ID-모델 명 디폴트
//
//     void Awake()
//     {
//         // 환신 첫 시 신고통 설정
//         if (Instance == null) Instance = this;
//         else
//         {
//             Destroy(gameObject);
//             return;
//         }
//
//         // 모델드림 초기화
//         monsterDB = monsterList.ToDictionary(m => m.id, m => m);
//
//         if (monsterDB.Count == 0)
//             Debug.LogError("❌ monsterDB 초기화 실패 - monsterList 비어있음");
//         else
//             Debug.Log($"✅ monsterDB 초기화 완료: {monsterDB.Count}개");
//     }
//
//     void Start()
//     {
//         LoadAllOffers();                                  // 사용 가능한 상점 파일 로드
//         refreshCount = PlayerPrefs.GetInt("refreshCount", 0); // 갱신 횟수 보관
//         CheckAutoRefresh();                               // 자동 갱신 체크
//         UpdateCostUI();                                   // 비용 UI 복잡
//     }
//
//     void Update()
//     {
//         if (Input.GetKeyDown(KeyCode.Backspace)) // 테스트용 초기화
//         {
//             refreshCount = 0;
//             PlayerPrefs.DeleteKey("refreshCount");
//             UpdateCostUI();
//             Debug.Log("🔄 리프레시 카운트 초기화됨");
//         }
//     }
//
//     private void ForceRefreshOnEnter()
//     {
//         ClearShopItems();                        // 상점 항목 지우기
//         currentOffers = GetRandomOffers();       // 복사 없이 보유
//         DisplayOffers(currentOffers);            // 표시
//         Debug.Log("[Shop] ✅ 첫 지방 시 상점 초기화 완료");
//     }
//
//     void CheckAutoRefresh()
//     {
//         string lastTimeStr = PlayerPrefs.GetString("lastRefreshTime", "");
//         DateTime now = DateTime.Now;
//
//         if (DateTime.TryParse(lastTimeStr, out DateTime lastTime))
//         {
//             TimeSpan diff = now - lastTime;
//
//             if (diff.TotalSeconds >= cooldownTime)
//             {
//                 Debug.Log($"[Shop] ⏰ 자동 갱신됨 - 경간 시간: {diff.TotalSeconds:F1}초");
//                 ForceRefreshOnEnter();
//                 PlayerPrefs.SetString("lastRefreshTime", now.ToString());
//             }
//             else
//             {
//                 Debug.Log($"[Shop] 💩 아직 갱신 안 되음 - 남은 시간: {cooldownTime - diff.TotalSeconds:F1}초");
//             }
//         }
//         else
//         {
//             Debug.Log("[Shop] 🆕 첫 갱신 시간 저장됨");
//             PlayerPrefs.SetString("lastRefreshTime", now.ToString());
//         }
//     }
//
//     void LoadAllOffers()
//     {
//         var json = Resources.Load<TextAsset>("shop_offers");
//
//         if (json == null)
//         {
//             Debug.LogError("[Shop] shop_offers.json 로드 실패!");
//             return;
//         }
//
//         var wrapper = JsonUtility.FromJson<ShopOfferListWrapper>(json.text);
//         allOffers = wrapper.offers;
//
//         if (allOffers == null || allOffers.Count == 0)
//         {
//             Debug.LogError("[Shop] offers 로드 실패 or 비어 있음!");
//         }
//         else
//         {
//             Debug.Log($"[Shop] 총 오퍼 수: {allOffers.Count}");
//         }
//
//         foreach (var offer in allOffers)
//         {
//             offer.item.iconPath = Resources.Load<Sprite>($"Icons/{offer.item.icon}");
//
//             if (Enum.TryParse<CurrencyType>(offer.currency, out var parsed))
//                 offer.currencyType = parsed;
//             else
//                 offer.currencyType = CurrencyType.Gold;
//         }
//     }
//
//     public void RefreshShop()
//     {
//         Debug.Log("[Shop] 🔁 RefreshShop 호출됨");
//
//         if (isCoolingDown)
//         {
//             Debug.LogWarning("[Shop] ❌ 쿠타임 중이라 리프레시 불가");
//             return;
//         }
//
//         CurrencyType currency;
//         int cost = GetRefreshCost(out currency);
//
//         if (!PlayerWallet.Instance.TrySpendCurrency(currency, cost))
//         {
//             Debug.LogWarning("[Shop] ❌ 재화 부족");
//             return;
//         }
//
//         refreshCount++;
//         PlayerPrefs.SetInt("refreshCount", refreshCount);
//         PlayerPrefs.SetString("lastRefreshTime", DateTime.Now.ToString());
//
//         Debug.Log("[Shop] ✅ 비용 참가 완료, 리프레시 시작");
//         Debug.Log($"[Shop] Refresh Count = {refreshCount}");
//
//         ClearShopItems();
//         currentOffers = GetRandomOffers();
//         DisplayOffers(currentOffers);
//
//         UpdateCostUI();
//         StartCoroutine(CooldownCoroutine());
//     }
//
//     int GetRefreshCost(out CurrencyType currency)
//     {
//         if (refreshCount < 10)
//         {
//             currency = CurrencyType.Gold;
//             return 100 + 50 * refreshCount;
//         }
//         else
//         {
//             currency = CurrencyType.Gem;
//             return 50 + 25 * (refreshCount - 10);
//         }
//     }
//
//     void UpdateCostUI()
//     {
//         CurrencyType currency;
//         int cost = GetRefreshCost(out currency);
//         costText.text = cost.ToString();
//         currencyIcon.sprite = CurrencyIconManager.GetSprite(currency);
//     }
//
//     void ClearShopItems()
//     {
//         foreach (Transform child in shopParent)
//             Destroy(child.gameObject);
//     }
//
//     List<ShopOfferData> GetRandomOffers()
//     {
//         return allOffers.OrderBy(x => Guid.NewGuid()).Take(displayCount).ToList();
//     }
//
//     void DisplayOffers(List<ShopOfferData> offers)
//     {
//         foreach (var offer in offers)
//         {
//             var slot = Instantiate(shopItemPrefab, shopParent);
//             slot.GetComponent<ShopItemUI>().SetOffer(
//                 offer,
//                 inventory,
//                 monsterDB,
//                 upgradeDB
//             );
//         }
//     }
//
//     IEnumerator CooldownCoroutine()
//     {
//         isCoolingDown = true;
//         refreshButtonImage.color = Color.gray;
//         currencyIcon.color = Color.gray;
//
//         float timeLeft = cooldownTime;
//         while (timeLeft > 0)
//         {
//             labelText.text = $"{Mathf.CeilToInt(timeLeft)}초";
//             timeLeft -= Time.deltaTime;
//             yield return null;
//         }
//
//         labelText.text = "새로고침";
//         refreshButtonImage.color = Color.white;
//         currencyIcon.color = Color.white;
//         isCoolingDown = false;
//     }
// }
