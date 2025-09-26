// // ✅ DeckDataSaver - 덱 저장/불러오기용
// using System.Collections.Generic;
// using UnityEngine;
// using Firebase.Auth;
// using Firebase.Firestore;
//
// [System.Serializable]
// public class SavedDeckEntry
// {
//     public string id;
//     public int level;
// }
//
// [System.Serializable]
// public class SavedDeckData
// {
//     public List<SavedDeckEntry> deck = new();
// }
//
// public class DeckDataSaver : MonoBehaviour
// {
//     public DeckManager_UI deckManager;
//     public PlayerCardInventory inventory;
//
//     private FirebaseAuth auth;
//     private FirebaseFirestore firestore;
//
//     private void Awake()
//     {
//         auth = FirebaseAuth.DefaultInstance;
//         firestore = FirebaseFirestore.DefaultInstance;
//     }
//
//     public void SaveDeckToFirebase()
//     {
//         string uid = auth.CurrentUser?.UserId;
//         if (string.IsNullOrEmpty(uid)) return;
//
//         var saveData = new SavedDeckData();
//         foreach (var card in deckManager.currentDeck)
//         {
//             saveData.deck.Add(new SavedDeckEntry
//             {
//                 id = card.id,
//                 level = card.level
//             });
//         }
//
//         firestore.Collection("users").Document(uid).Collection("decks").Document("main").SetAsync(saveData);
//         Debug.Log("[Deck] ✅ 덱 저장 완료");
//     }
//
//     public void LoadDeckFromFirebase()
//     {
//         string uid = auth.CurrentUser?.UserId;
//         if (string.IsNullOrEmpty(uid)) return;
//
//         firestore.Collection("users").Document(uid).Collection("decks").Document("main").GetSnapshotAsync().ContinueWith(task =>
//         {
//             if (!task.Result.Exists)
//             {
//                 Debug.LogWarning("[Deck] ⚠️ 저장된 덱이 없음");
//                 return;
//             }
//
//             var json = task.Result.ConvertTo<SavedDeckData>();
//             if (json == null) return;
//
//             deckManager.currentDeck.Clear();
//
//             foreach (var entry in json.deck)
//             {
//                 var card = inventory.GetCardData(entry.id);
//                 if (card != null)
//                 {
//                     deckManager.currentDeck.Add(card);
//                 }
//             }
//
//             Debug.Log("[Deck] ✅ 덱 불러오기 완료");
//             deckManager.RefreshDeckUI();
//         });
//     }
// }
