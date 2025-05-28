using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class DeckManager_UI : MonoBehaviour
{
    public static DeckManager_UI Instance;
    
    [SerializeField] private Transform[] slotPositions;
    public GameObject slotPrefab;
    public List<PlayerCardData> currentDeck = new();
    public int maxDeckSize = 6;

    public CollectionPanel collectionPanel;
    public UpgradeRequirementDB upgradeDB;
    public PlayerCardInventory inventory;

    public bool isReplaceMode;
    public PlayerCardData replaceTargetCard;
    
    [SerializeField] private MonsterData_Mainmenu[] monsterList;

    private Dictionary<string, MonsterData_Mainmenu> monsterDB;

    void Awake()
    {
        Instance = this; // ✅ 반드시 있어야 합니다
    }

    void Start()
    {
        MonsterDatabase.Initialize();

        currentDeck.Clear();
        currentDeck.Add(new PlayerCardData("Golem", 7));
        currentDeck.Add(new PlayerCardData("Mage", 7));
        currentDeck.Add(new PlayerCardData("Rogue", 7));
        currentDeck.Add(new PlayerCardData("Necro", 1));
        currentDeck.Add(new PlayerCardData("Minion", 1));
        currentDeck.Add(new PlayerCardData("Warrior", 1));

        InitializeCardData(); // ✅ id 기반으로 monsterData 자동 연결
        RefreshDeckUI();
    }
    
    public void TryAddCard(PlayerCardData card)
    {
        if (currentDeck.Any(c => c.id == card.id)) return;
        if (currentDeck.Count >= maxDeckSize) return;

        currentDeck.Add(card);
        RefreshDeckUI();
        collectionPanel.Refresh();
    }

    public void RemoveCard(int index)
    {
        if (index < 0 || index >= currentDeck.Count) return;

        var removedCard = currentDeck[index];
        currentDeck.RemoveAt(index);

        collectionPanel.ReturnCard(removedCard);

        RefreshDeckUI();
        collectionPanel.Refresh();
    }

    public void RefreshDeckUI()
    {
        foreach (var pos in slotPositions)
        {
            foreach (Transform child in pos)
                Destroy(child.gameObject);
        }

        for (int i = 0; i < currentDeck.Count; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, slotPositions[i], false);
            var slot = slotGO.GetComponent<SlotUI>();
            slot.Setup(currentDeck[i], i, this, SlotMode.Deck);
        }
    }
    public static class MonsterDatabase
    {
        public static Dictionary<string, MonsterData_Mainmenu> monsterDataDict;

        public static void Initialize()
        {
            monsterDataDict = new Dictionary<string, MonsterData_Mainmenu>();

            // 예시: 리소스에서 불러오거나 수동으로 등록
            var allMonsters = Resources.LoadAll<MonsterData_Mainmenu>("Monsters");
            foreach (var data in allMonsters)
            {
                monsterDataDict[data.id] = data; // 가정: MonsterData_Mainmenu에 id 필드 있음
            }
        }

        public static MonsterData_Mainmenu GetMonsterDataById(string id)
        {
            monsterDataDict.TryGetValue(id, out var data);
            return data;
        }
    }
    public void InitializeCardData()
    {
        foreach (var card in currentDeck)
        {
            if (card.monsterData == null)
            {
                card.monsterData = MonsterDatabase.GetMonsterDataById(card.id);
                if (card.monsterData == null)
                    Debug.LogWarning($"[InitializeCardData] monsterData 찾을 수 없음: {card.id}");
            }
        }
    }
    
    public bool IsInDeck(string id) => currentDeck.Any(c => c.id == id);

    public void TryAddOrReplace(PlayerCardData card)
    {
        if (IsInDeck(card.id)) return;

        if (currentDeck.Count < maxDeckSize)
        {
            currentDeck.Add(card);
            RefreshDeckUI();
            collectionPanel.Refresh();
        }
        else
        {
            isReplaceMode = true;
            replaceTargetCard = card;
            Debug.Log($"[DeckManager] 교체 모드 진입: {card.monsterData.monsterName}");

            RefreshDeckUI();
        }
    }

    public void ReplaceCard(int index)
    {
        if (!isReplaceMode || replaceTargetCard == null) return;

        var oldCard = currentDeck[index];
        currentDeck[index] = replaceTargetCard;

        collectionPanel.ReturnCard(oldCard);
        collectionPanel.RemoveCardFromCollection(replaceTargetCard);

        isReplaceMode = false;
        replaceTargetCard = null;

        RefreshDeckUI();
        collectionPanel.Refresh();
    }
}