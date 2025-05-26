// ✅ InventoryUtility.cs - 초기 카드 생성 + Firebase 저장/불러오기
using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using System.Collections.Generic;

public class InventoryUtility : MonoBehaviour
{
    public PlayerCardInventory inventory;
    public MonsterData_Mainmenu[] initialCardList; // 초기에 지급할 카드들

    private FirebaseAuth auth;
    private FirebaseFirestore firestore;

    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;
    }

    [ContextMenu("💠 초기 카드 생성 및 세팅")]
    public void CreateDefaultInventory()
    {
        inventory.allOwnedCards.Clear();

        foreach (var card in initialCardList)
        {
            inventory.allOwnedCards.Add(new PlayerCardData(
                card.id,
                level: 1,
                ownedCount: 20
            )
            {
                monsterData = card
            });
        }

        Debug.Log($"✅ 초기 카드 {inventory.allOwnedCards.Count}개 생성 완료");
    }

    [ContextMenu("☁️ 인벤토리 저장 to Firebase")]
    public void SaveInventoryToFirebase()
    {
        string uid = auth.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(uid)) return;

        var list = new List<Dictionary<string, object>>();
        foreach (var card in inventory.allOwnedCards)
        {
            list.Add(new Dictionary<string, object>
            {
                { "id", card.id },
                { "level", card.level },
                { "ownedCount", card.ownedCount }
            });
        }

        firestore.Collection("users").Document(uid)
            .Collection("data").Document("inventory")
            .SetAsync(new Dictionary<string, object> { { "cards", list } });

        Debug.Log("☁️ 인벤토리 저장 완료");
    }

    [ContextMenu("☁️ 인벤토리 불러오기 from Firebase")]
    public void LoadInventoryFromFirebase(Dictionary<string, MonsterData_Mainmenu> monsterDB)
    {
        string uid = auth.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(uid)) return;

        firestore.Collection("users").Document(uid)
            .Collection("data").Document("inventory")
            .GetSnapshotAsync().ContinueWith(task =>
        {
            if (!task.Result.Exists)
            {
                Debug.LogWarning("⚠️ 인벤토리 데이터 없음");
                return;
            }

            var json = task.Result.ToDictionary();
            var list = json["cards"] as List<object>;

            inventory.allOwnedCards.Clear();
            foreach (var raw in list)
            {
                var dict = raw as Dictionary<string, object>;
                string id = dict["id"].ToString();
                int level = int.Parse(dict["level"].ToString());
                int count = int.Parse(dict["ownedCount"].ToString());

                if (monsterDB.TryGetValue(id, out var monster))
                {
                    inventory.allOwnedCards.Add(new PlayerCardData(id, level, count)
                    {
                        monsterData = monster
                    });
                }
            }

            Debug.Log($"☁️ 인벤토리 {inventory.allOwnedCards.Count}개 불러오기 완료");
        });
    }
}
