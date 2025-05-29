using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;
    private int maxCardNum = 6;
    public List<BaseData> currentPlayerDeck = new List<BaseData>();
    public List<BaseData> totalPlayerDeck = new List<BaseData>(); // 총덱
    private FirebaseFirestore firestore;

    public CollectionPanel collectionPanel;
    
    public event Action OnCurrentDeckReady;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        firestore = FirebaseFirestore.DefaultInstance;
    }
    private async void Start()
    {
        await WaitForAuth();
        totalPlayerDeck.Add(Resources.Load<BaseData>("BaseData/Warrior"));
        totalPlayerDeck.Add(Resources.Load<BaseData>("BaseData/Golem"));
        totalPlayerDeck.Add(Resources.Load<BaseData>("BaseData/Mage"));
        totalPlayerDeck.Add(Resources.Load<BaseData>("BaseData/ArrowRain"));
        totalPlayerDeck.Add(Resources.Load<BaseData>("BaseData/Rogue"));
        totalPlayerDeck.Add(Resources.Load<BaseData>("BaseData/Minion"));
        totalPlayerDeck.Add(Resources.Load<BaseData>("BaseData/Necromancer"));
        totalPlayerDeck.Add(Resources.Load<BaseData>("BaseData/Cannon"));
        totalPlayerDeck.Add(Resources.Load<BaseData>("BaseData/Fireball"));
        await FirstSetCurrentDeckDic();
        //await FirstTotalDeckDataSet();
    }
    public async Task FirstSetCurrentDeckDic() // 데이터가 없을때 처음 시작할때 기본덱 저장
    {
        DocumentSnapshot snapshot = await GetTotalPlayerDeckDocRef("totalPlayerDeck").GetSnapshotAsync();
        if (snapshot.Exists == false)
        {
            for (int i = 0; i < maxCardNum; i++)
            {
                totalPlayerDeck[i].isUsed = true;
                currentPlayerDeck.Add(totalPlayerDeck[i]);
            }
            // 초기화 후 저장
            DeckSaveLoad_Firebase.Instance.DeckSave_FireBase(FirebaseAuth.DefaultInstance.CurrentUser.UserId.ToString(), totalPlayerDeck,  "totalPlayerDeck");
            OnCurrentDeckReady?.Invoke();
        }
        else
        {
            // 있으면 불러오기
            DeckSaveLoad_Firebase.Instance.DeckLoad_FireBase(FirebaseAuth.DefaultInstance.CurrentUser.UserId.ToString(), "totalPlayerDeck", false,
                (loadDeck) =>
                {
                    currentPlayerDeck = loadDeck;
                    OnCurrentDeckReady?.Invoke();
                });
            DeckSaveLoad_Firebase.Instance.DeckLoad_FireBase(FirebaseAuth.DefaultInstance.CurrentUser.UserId.ToString(), "totalPlayerDeck", true,
               (loadDeck) =>
               {
                   totalPlayerDeck = loadDeck;
               });
        }
        //OnCurrentDeckReady?.Invoke();
    }
    private DocumentReference  GetTotalPlayerDeckDocRef(string documentName)
    {
        return firestore
            .Collection("users")
            .Document(FirebaseAuth.DefaultInstance.CurrentUser.UserId.ToString())
            .Collection("deck")
            .Document(documentName);
    }
    private async Task WaitForAuth()
    {
        while (FirebaseAuth.DefaultInstance.CurrentUser == null)
        {
            await Task.Delay(100);
        }
    }

    public void DeckSave() // 종료시 저장
    {
        DeckSaveLoad_Firebase.Instance.DeckSave_FireBase(FirebaseAuth.DefaultInstance.CurrentUser.UserId.ToString(), currentPlayerDeck, "totalPlayerDeck");
    }
}
