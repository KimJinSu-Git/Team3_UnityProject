using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System;
using System.Collections.Generic;

public class PlayerWallet : MonoBehaviour
{
    public static PlayerWallet Instance { get; private set; }

    public int gold { get; private set; }
    public int gem { get; private set; }

    private FirebaseAuth auth;
    private FirebaseFirestore firestore;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;
    }

    public void LoadFromFirebase()
    {
        string uid = auth.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(uid))
        {
            Debug.LogWarning("로그인된 유저가 없습니다.");
            return;
        }

        firestore.Collection("users").Document(uid).GetSnapshotAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                // data 불러오기
                var data = task.Result.ToDictionary();

                gold = data.ContainsKey("gold") ? Convert.ToInt32(data["gold"]) : 0;
                gem = data.ContainsKey("gem") ? Convert.ToInt32(data["gem"]) : 0;

                Debug.Log($"[Wallet] 로드 완료: Gold={gold}, Gem={gem}");
            }
            //신규라면 초기 자금 지급
            else
            {
                Debug.Log("[Wallet] 신규 유저이므로 기본값 설정");
                gold = 1000;
                gem = 100;
                SaveToFirebase(); // 초기화 후 저장
            }
        });
    }

    public void SaveToFirebase()
    {
        string uid = auth.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(uid)) return;

        // data 저장
        var data = new Dictionary<string, object>
        {
            { "gold", gold },
            { "gem", gem }
        };

        firestore.Collection("users").Document(uid).SetAsync(data, SetOptions.MergeAll);
    }

    // ====== 사용 메서드 ======

    public bool HasEnoughGold(int amount) => gold >= amount;
    public bool HasEnoughGem(int amount) => gem >= amount;

    public void SpendGold(int amount)
    {
        gold -= amount;
        SaveToFirebase();
    }

    public void SpendGem(int amount)
    {
        gem -= amount;
        SaveToFirebase();
    }

    public void AddGold(int amount)
    {
        gold += amount;
        SaveToFirebase();
    }

    public void AddGem(int amount)
    {
        gem += amount;
        SaveToFirebase();
    }
}
