using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;


public class DeckSaveLoad_Firebase : MonoBehaviour
{
    public static DeckSaveLoad_Firebase Instance;
    private FirebaseFirestore firestore;
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        firestore = FirebaseFirestore.DefaultInstance;
    }
    public void DeckSave_FireBase(string UserId, List<BaseData> deck, string documentName) // 처음에 모든 카드 정보 저장
    {
        List<Dictionary<string, object>> DeckList = new List<Dictionary<string, object>>();
        foreach (BaseData card in deck)
        {
            Dictionary<string, object> cardDic = new Dictionary<string, object>();
            cardDic.Add("cardID", card.id);
            cardDic.Add("cardName", card.cardName);
            cardDic.Add("damage", card.damage);
            cardDic.Add("ownedCardCount", card.ownedCardCount);
            cardDic.Add("isUsed", card.isUsed);
            if (card is MonsterData monsterCard)
            {
                cardDic.Add("maxHP", monsterCard.maxHP);
            }
            DeckList.Add(cardDic);
        }
        
        Dictionary<string, object> deckData = new Dictionary<string, object>
        {
            { "cards", DeckList }
        };
        firestore.Collection("users").Document(UserId).Collection("deck").Document(documentName).SetAsync(deckData);
    }

    public async void DeckLoad_FireBase(string userId, string documnetName,bool totalDeck, Action<List<BaseData>> onDeckLoaded) // 모든 카드데이터 현제 스텟데이터 세팅해서 가져옴
    {
        DocumentReference doRef = firestore.Collection("users").Document(userId).Collection("deck").Document(documnetName);
        
        DocumentSnapshot snapshot = await doRef.GetSnapshotAsync();
        if (snapshot.Exists)
        {
            if (snapshot.TryGetValue("cards", out List<object> deckData))
            {
                List<BaseData> deckList = new List<BaseData>();
                // 리스트를 가져와서 안에있는 딕셔너리 하나씩 꺼내줌
                foreach (var card in deckData)
                {
                    Dictionary<string, object> cardDic = card as Dictionary<string, object>;
                    string cardName = cardDic["cardName"].ToString();
                    // 이름으로 스크립터블 오브젝트 찾아서 넣어줌
                    BaseData baseData = Resources.Load<BaseData>($"BaseData/{cardName}");
                    //넣으면서 값 세팅
                    baseData.damage = Convert.ToInt32(cardDic["damage"]);
                        baseData.ownedCardCount = Convert.ToInt32(cardDic["ownedCardCount"]);
                    if (baseData is MonsterData monsterCard)
                    {
                        monsterCard.maxHP = Convert.ToInt32(cardDic["maxHP"]);
                    }
                    baseData.isUsed = Convert.ToBoolean(cardDic["isUsed"]);

                    if (totalDeck == false)
                    {
                        if (baseData.isUsed == true)
                        {
                            deckList.Add(baseData);
                        }
                    }
                    else
                    {
                        deckList.Add(baseData);
                    }
                }
                // 넘겨줌
                onDeckLoaded?.Invoke(deckList);
            }
        }
    }
    void Update()
    {
        
    }
}
