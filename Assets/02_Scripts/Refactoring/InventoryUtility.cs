using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;

public class InventoryUtility : MonoBehaviour
{
    public static InventoryUtility Instance;
    
    [Header("데이터")]
    public PlayerCardInventory inventory;
    
    [Header("몬스터 목록")]
    [SerializeField] private MonsterData[] monsterList;
    public Dictionary<string, MonsterData> monsterDB;
    [SerializeField] private SkillData[] skillList;
    public Dictionary<string, SkillData> skillDB;

    private FirebaseAuth auth;
    private FirebaseFirestore firestore;

    void Awake()
    {
        // 싱글톤 할당
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;

        // ✅ monsterDB 초기화
        monsterDB = monsterList.ToDictionary(m => m.id, m => m);
    }

    [ContextMenu("☁️ 인벤토리 불러오기 from Firebase")]
    public void LoadInventoryFromFirebase(Action onComplete = null)
    {
        string uid = auth.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(uid))
        {
            Debug.LogWarning("❌ 로그인된 유저가 없습니다.");
            return;
        }

        firestore.Collection("users").Document(uid)
            .Collection("data").Document("inventory")
            .GetSnapshotAsync().ContinueWith(task =>
        {
            if (!task.Result.Exists)
            {
                Debug.LogWarning("⚠️ Firebase 인벤토리 없음 → 초기화 진행");

                CreateDefaultInventory(monsterDB, skillDB);
                SaveInventoryToFirebase();

                onComplete?.Invoke();
                return;
            }

            var rawData = task.Result.ToDictionary();
            var cardList = rawData["cards"] as List<object>;

            inventory.allOwnedCards.Clear();

            foreach (var raw in cardList)
            {
                var dict = raw as Dictionary<string, object>;
                string id = dict["id"].ToString();
                int level = Convert.ToInt32(dict["level"]);
                int count = Convert.ToInt32(dict["ownedCount"]);

                if (monsterDB.TryGetValue(id, out var monster))
                {
                    inventory.allOwnedCards.Add(new PlayerCardData(id, level, count)
                    {
                        monsterData = monster
                    });
                }
                else
                {
                    Debug.LogWarning($"❌ MonsterData 없음: {id}");
                }
            }

            Debug.Log($"☁️ Firebase 인벤토리 불러오기 완료: {inventory.allOwnedCards.Count}개");
            onComplete?.Invoke();
        });
    }

    public void CreateDefaultInventory(Dictionary<string, MonsterData> monsterDB, Dictionary<string, SkillData> skillDB)
    {
        inventory.allOwnedCards.Clear();

        foreach (var monster in monsterDB.Values)
        {
            inventory.allOwnedCards.Add(new PlayerCardData(monster.id, 1, 20)
            {
                monsterData = monster
            });
        }

        foreach (var skill in skillDB.Values)
        {
            inventory.allOwnedCards.Add(new PlayerCardData(skill.id, 1, 20)
            {
                skillData = skill
            });
        }

        Debug.Log($"✅ 기본 카드 {inventory.allOwnedCards.Count}개 생성 완료");
    }

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
}
