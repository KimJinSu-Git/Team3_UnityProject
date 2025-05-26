using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System;
using System.Collections.Generic;

public enum CurrencyType
{
    Gold,
    Gem
}

public class PlayerWallet : MonoBehaviour
{
    public static PlayerWallet Instance { get; private set; }

    public int gold { get; private set; }
    public int gem { get; private set; }

    private FirebaseAuth auth;
    private FirebaseFirestore firestore;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;
    }

    public void LoadFromFirebase()
    {
        string uid = auth.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(uid))
        {
            Debug.LogWarning("[Wallet] 로그인된 유저가 없습니다.");
            return;
        }

        firestore.Collection("users").Document(uid).GetSnapshotAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                var data = task.Result.ToDictionary();

                gold = data.ContainsKey("gold") ? Convert.ToInt32(data["gold"]) : 0;
                gem = data.ContainsKey("gem") ? Convert.ToInt32(data["gem"]) : 0;

                Debug.Log($"[Wallet] 로드 완료: Gold={gold}, Gem={gem}");
            }
            else
            {
                Debug.Log("[Wallet] 신규 유저로 기본값 설정");
                gold = 1000;
                gem = 100;
                SaveToFirebase();
            }
        });
    }

    public void SaveToFirebase()
    {
        string uid = auth.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(uid)) return;

        var data = new Dictionary<string, object>
        {
            { "gold", gold },
            { "gem", gem }
        };

        firestore.Collection("users").Document(uid).SetAsync(data, SetOptions.MergeAll);
    }

    // ====== 재화 공통 메서드 ======

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
    }

    public int GetCurrencyAmount(CurrencyType type)
    {
        return type switch
        {
            CurrencyType.Gold => gold,
            CurrencyType.Gem => gem,
            _ => 0
        };
    }

    public bool HasEnoughCurrency(CurrencyType type, int amount)
    {
        return GetCurrencyAmount(type) >= amount;
    }

    public bool TrySpendGold(int amount)
    {
        if (gold < amount) return false;
        gold -= amount;
        SaveToFirebase();
        return true;
    }

    public bool TrySpendGem(int amount)
    {
        if (gem < amount) return false;
        gem -= amount;
        SaveToFirebase();
        return true;
    }
}
