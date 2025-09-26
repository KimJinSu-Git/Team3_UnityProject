using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using TMPro;
using Firebase.Extensions;
using UnityEngine.PlayerLoop;


/// 💳 플레이어의 골드/젬 재화 및 UI/Firebase 관리
public class PlayerWallet : MonoBehaviour
{
    public static PlayerWallet Instance { get; private set; } // 싱글톤 인스턴스

    public int gold { get; private set; } // 현재 골드
    public int gem { get; private set; }  // 현재 젬

    private FirebaseAuth auth; // Firebase 인증
    //private FirebaseFirestore firestore; // Firebase Firestore DB

    [Header("UI 연결")]
    [SerializeField] private TextMeshProUGUI goldText; // 골드 표시 UI
    [SerializeField] private TextMeshProUGUI gemText;  // 젬 표시 UI

    private void Awake()
    {
        // ✅ 싱글톤 생성
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // ✅ Firebase 참조 설정
        auth = FirebaseAuth.DefaultInstance;
        //firestore = FirebaseFirestore.DefaultInstance;
    }
    
    private void OnEnable()
    {
        UpdateUI(); // UI가 켜질 때마다 값 반영
    }
    
    void Start()
    {
        Debug.Log("[Wallet] Start() 실행됨 → LoadFromFirebase 호출 시도");

        if (Instance != null)
            Instance.LoadFromFirebase();
        else
            Debug.LogWarning("[Wallet] Instance가 null입니다. Load 실패");
    }

    /// ☁️ Firebase에서 유저 재화 불러오기
    public void LoadFromFirebase()
    {
        string uid = auth.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(uid))
        {
            Debug.LogWarning("[Wallet] 로그인된 유저가 없습니다.");
            return;
        }

        FirestoreManager.Instance.firestore.Collection("users").Document(uid).GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && task.Result.Exists)
                {
                    var data = task.Result.ToDictionary();
                    gold = data.ContainsKey("gold") ? Convert.ToInt32(data["gold"]) : 0;
                    gem = data.ContainsKey("gem") ? Convert.ToInt32(data["gem"]) : 0;

                    Debug.Log($"[Wallet] 로드 완료: Gold={gold}, Gem={gem}");
                    UpdateUI(); // ✅ 이제 UI에 정상 반영됨
                }
                else
                {
                    Debug.Log("[Wallet] 신규 유저로 기본값 설정");
                    gold = 1000;
                    gem = 100;
                    SaveToFirebase();
                    UpdateUI();
                }
            });
    }

    /// ☁️ Firebase에 현재 재화 저장
    public void SaveToFirebase()
    {
        string uid = auth.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(uid)) return;

        var data = new Dictionary<string, object>
        {
            { "gold", gold },
            { "gem", gem }
        };

        FirestoreManager.Instance.firestore.Collection("users").Document(uid).SetAsync(data, SetOptions.MergeAll);
    }

    // ===================== 재화 처리 공통 메서드 =====================

    /// 💸 특정 재화 차감 시도
    public bool TrySpendCurrency(CurrencyType type, int amount)
    {
        switch (type)
        {
            case CurrencyType.Gold:
                return TrySpendGold(amount);
            case CurrencyType.Gem:
                return TrySpendGem(amount);
            default:
                return false;
        }
    }

    /// 💰 특정 재화 추가
    public void AddCurrency(CurrencyType type, int amount)
    {
        switch (type)
        {
            case CurrencyType.Gold:
                gold += amount;
                break;
            case CurrencyType.Gem:
                gem += amount;
                break;
        }
        SaveToFirebase();
        UpdateUI();
    }

    /// 💲 특정 재화의 현재 값 반환
    public int GetCurrencyAmount(CurrencyType type)
    {
        return type switch
        {
            CurrencyType.Gold => gold,
            CurrencyType.Gem => gem,
            _ => 0
        };
    }

    /// ✅ 충분한 재화 보유 여부 확인
    public bool HasEnoughCurrency(CurrencyType type, int amount)
    {
        return GetCurrencyAmount(type) >= amount;
    }

    /// 💳 골드 차감 시도
    public bool TrySpendGold(int amount)
    {
        if (gold < amount) return false;
        gold -= amount;
        SaveToFirebase();
        UpdateUI();
        return true;
    }

    /// 💎 젬 차감 시도
    public bool TrySpendGem(int amount)
    {
        if (gem < amount) return false;
        gem -= amount;
        SaveToFirebase();
        UpdateUI();
        return true;
    }

    /// 🔄 UI 업데이트 (Text에 값 반영)
    private void UpdateUI()
    {
        Debug.Log($"[Wallet UI] goldText={(goldText == null ? "❌ NULL" : "✅ OK")}, gemText={(gemText == null ? "❌ NULL" : "✅ OK")}");

        if (goldText != null)
            goldText.text = gold.ToString();

        if (gemText != null)
            gemText.text = gem.ToString();
    }
}
