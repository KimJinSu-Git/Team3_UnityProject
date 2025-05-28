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
    
    [SerializeField] private MonsterData[] monsterList;
    private Dictionary<string, MonsterData> monsterDB;
    
    [SerializeField] private SkillData[] skillList;
    private Dictionary<string, SkillData> skillDB;

    void Awake()
    {
        Instance = this; // ✅ 반드시 있어야 합니다
    }

    void Start()
    {
        MonsterDatabase.Initialize();
        SkillDatabase.Initialize(); // ✅ 스킬 데이터베이스도 초기화

        currentDeck.Clear();
        currentDeck.Add(new PlayerCardData("1", 7));
        currentDeck.Add(new PlayerCardData("8", 7));
        currentDeck.Add(new PlayerCardData("3", 7));
        currentDeck.Add(new PlayerCardData("4", 1));
        currentDeck.Add(new PlayerCardData("6", 1));
        currentDeck.Add(new PlayerCardData("5", 1));

        InitializeCardData(); // ✅ id 기반으로 monsterData와 skillData 자동 연결
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

    // ✅ 몬스터 데이터베이스
    public static class MonsterDatabase
    {
        public static Dictionary<string, MonsterData> monsterDataDict;

        public static void Initialize()
        {
            monsterDataDict = new Dictionary<string, MonsterData>();

            // 예시: 리소스에서 불러오거나 수동으로 등록
            var allMonsters = Resources.LoadAll<MonsterData>("Monsters");
            foreach (var data in allMonsters)
            {
                monsterDataDict[data.id] = data; // 가정: MonsterData에 id 필드 있음
            }
        }

        public static MonsterData GetMonsterDataById(string id)
        {
            monsterDataDict.TryGetValue(id, out var data);
            return data;
        }
    }

    // ✅ 스킬 데이터베이스 추가
    public static class SkillDatabase
    {
        public static Dictionary<string, SkillData> skillDataDict;

        public static void Initialize()
        {
            skillDataDict = new Dictionary<string, SkillData>();

            // 리소스에서 스킬 데이터 로드
            var allSkills = Resources.LoadAll<SkillData>("Skills");
            foreach (var data in allSkills)
            {
                skillDataDict[data.id] = data; // 가정: SkillData에 id 필드 있음
            }
        }

        public static SkillData GetSkillDataById(string id)
        {
            skillDataDict.TryGetValue(id, out var data);
            return data;
        }
    }

    public void InitializeCardData()
    {
        foreach (var card in currentDeck)
        {
            // ✅ 몬스터 데이터 초기화
            if (card.monsterData == null)
            {
                card.monsterData = MonsterDatabase.GetMonsterDataById(card.id);
                if (card.monsterData == null)
                    Debug.LogWarning($"[InitializeCardData] monsterData 찾을 수 없음: {card.id}");
            }

            // ✅ 스킬 데이터 초기화 (로컬 데이터베이스 우선, 백업으로 SkillManager 사용)
            if (card.skillData == null)
            {
                // 먼저 로컬 데이터베이스에서 찾기
                card.skillData = SkillDatabase.GetSkillDataById(card.id);
                
                // 없으면 SkillManager에서 찾기
                if (card.skillData == null)
                {
                    card.skillData = SkillManager.Instance.GetSkillData(card.id);
                }
                
                if (card.skillData == null)
                    Debug.LogWarning($"[InitializeCardData] 스킬 데이터를 찾을 수 없음: {card.id}");
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
            
            // ✅ 카드 타입에 따른 이름 표시
            string cardName = card.monsterData?.monsterName ?? card.skillData?.skillName ?? "Unknown Card";
            Debug.Log($"[DeckManager] 교체 모드 진입: {cardName}");

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