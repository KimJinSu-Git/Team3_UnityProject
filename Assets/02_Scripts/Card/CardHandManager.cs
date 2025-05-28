using System;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public static class InGameLoader
{
    public static List<CardDataWrapper> playerDeckToLoad;
}

public class CardHandManager : MonoBehaviour
{
    public static CardHandManager Instance;

    [Header("덱 설정")]
    public List<CardDataWrapper> fullDeck; // Monster or Skill 카드 모두 포함
    private List<CardDataWrapper> deck;    // 런타임용 덱
    public Dictionary<string, MonsterData> monsterDatas; // 스텟등 확정성을 위해서 MonsterData 전부를 넘겨줌
    public Dictionary<string, SkillData> skillDatas;     // ✅ 스킬 데이터도 동일하게 관리

    [Header("UI 슬롯 & 프리팹")]
    public CardUI      cardPrefab;         // 카드 UI 프리팹
    public Transform[] slotParents;        // 손패 슬롯 부모(4개)
    public Transform   sideSlotParent;     // 사이드 슬롯 부모(5번째)

    [Header("사이드 슬롯 스케일 & 타이밍")]
    public Vector3     sideScale        = new Vector3(0.5f, 0.5f, 1f);
    public float       sideDelay        = 2f;   // 사이드→손패 전환 대기 시간
    public float       sideAnimDuration = 0.5f; // 이동 애니메이션 시간

    [Header("스포너")]
    public Spawner_Network unitSpawner;
    
    [Header("적 진영 영역 (Host용)")]
    public Collider[] hostEnemyZones;

    [Header("적 진영 영역 (Client용)")]
    public Collider[] clientEnemyZones;
    
    [SerializeField] private Collider[] currentNoSpawnZones;
    [SerializeField] private Image[] currentEnemyAreaImages;

    private List<CardUI> hand = new List<CardUI>(); // 현재 손패
    private CardUI       sideCard;                  // 사이드 슬롯 카드

    public Transform Area;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        var runner = FindObjectOfType<NetworkRunner>();
        
        // Host/Client 여부에 따라 콜라이더 세팅
        if (runner != null && runner.IsServer)
        {
            Debug.Log("Host에용");
            currentNoSpawnZones = hostEnemyZones;
        }
        else
        {
            Debug.Log("Client에용");
            currentNoSpawnZones = clientEnemyZones;
        }
        
        foreach (var img in currentEnemyAreaImages)
            img.enabled = false;
        
        // ✅ 외부 전달 덱이 있으면 주입
        if ((fullDeck == null || fullDeck.Count == 0) && InGameLoader.playerDeckToLoad != null)
            fullDeck = InGameLoader.playerDeckToLoad;

        if (fullDeck == null || fullDeck.Count == 0)
        {
            Debug.LogError("[CardHandManager] fullDeck이 비어있습니다.");
            return;
        }

        deck = new List<CardDataWrapper>(fullDeck);
        Shuffle(deck);

        // ✅ 몬스터 데이터 캐싱
        monsterDatas = new Dictionary<string, MonsterData>();
        foreach (var data in deck)
        {
            if (data.IsMonster && data.monsterData != null && !monsterDatas.ContainsKey(data.monsterData.name))
            {
                monsterDatas.Add(data.monsterData.name, data.monsterData);
            }
        }
        
        // ✅ 스킬 데이터 캐싱 (수정됨)
        skillDatas = new Dictionary<string, SkillData>();
        foreach (var data in deck)
        {
            if (data.IsSkill && data.skillData != null && !skillDatas.ContainsKey(data.skillData.name))
            {
                skillDatas.Add(data.skillData.name, data.skillData);
            }
        }

        Debug.Log($"[CardHandManager] 캐시된 몬스터: {monsterDatas.Count}개, 스킬: {skillDatas.Count}개");

        // ✅ 초기 손패 4장 뽑기
        for (int i = 0; i < slotParents.Length; i++)
            DrawToSlot(i);

        // ✅ 사이드 슬롯에 5번째 카드 배치
        DrawToSideSlot();
    }
    
    private void DrawToSlot(int slotIndex)   // 지정된 슬롯에 덱 맨 앞 카드를 뽑아서 UI 생성
    {
        if (deck.Count == 0) return;

        // 덱에서 카드 데이터 꺼내기
        CardDataWrapper data = deck[0];
        deck.RemoveAt(0);

        // 카드 인스턴스 생성 및 초기화
        CardUI card = Instantiate(cardPrefab);
        if (data.IsMonster)
        {
            card.Init(data.monsterData, null, data.cardType,
                unitSpawner, currentNoSpawnZones, currentEnemyAreaImages,
                slotParents[slotIndex], slotIndex, OnCardPlayed, Area,
                true, Vector3.one);
        }
        else if (data.IsSkill)
        {
            card.Init(null, data.skillData, data.cardType,
                unitSpawner, currentNoSpawnZones, currentEnemyAreaImages,
                slotParents[slotIndex], slotIndex, OnCardPlayed, Area,
                true, Vector3.one);
        }

        hand.Insert(slotIndex, card);
    }
    
    private void DrawToSideSlot()    // 사이드 슬롯에 카드 하나 배치 (드래그 불가, 작은 크기)
    {
        Debug.Log($"[CardHandManager] DrawToSideSlot: deckCount={deck.Count}");
        if (deck.Count == 0) return;

        // 덱에서 카드 데이터 꺼내기
        CardDataWrapper data = deck[0];
        deck.RemoveAt(0);
        
        string cardName = data.IsMonster ? data.monsterData.monsterName : data.skillData.skillName;
        Debug.Log($"[CardHandManager] drawing new side: {cardName}");
        
        // 기존 사이드 카드가 있으면 삭제 (사이드 슬롯에 남아 있는 경우만)
        if (sideCard != null && sideCard.transform.parent == sideSlotParent)
            Destroy(sideCard.gameObject);

        // 새 사이드 카드 인스턴스 생성 및 초기화
        sideCard = Instantiate(cardPrefab);
        if (data.IsMonster)
        {
            sideCard.Init(data.monsterData, null, data.cardType,
                unitSpawner, currentNoSpawnZones, currentEnemyAreaImages,
                sideSlotParent, -1, null, Area,
                false, sideScale);
        }
        else if (data.IsSkill)
        {
            sideCard.Init(null, data.skillData, data.cardType,
                unitSpawner, currentNoSpawnZones, currentEnemyAreaImages,
                sideSlotParent, -1, null, Area,
                false, sideScale);
        }
    }
    
    private void OnCardPlayed(int slotIndex)    // 카드 유닛 배치 시 호출되는 콜백
    {
        Debug.Log($"[CardHandManager] OnCardPlayed({slotIndex}) called. Current sideCard={(sideCard!=null? sideCard.name:"NULL")}");
        StartCoroutine(MoveSideToHand(slotIndex));
    }
    
    private IEnumerator MoveSideToHand(int slotIndex)
    {
        // 1) 플레이된 손패 카드 UI & 데이터 처리
        if (slotIndex < 0 || slotIndex >= hand.Count)
            yield break;

        // 1-a) 플레이된 카드 데이터 캡처 → 덱 맨 뒤에 추가
        CardUI playedCardUI = hand[slotIndex];
        var playedData = new CardDataWrapper {
            cardType    = playedCardUI.MonsterData != null
                ? CardDataWrapper.CardType.Monster
                : CardDataWrapper.CardType.Skill,
            monsterData = playedCardUI.MonsterData,
            skillData   = playedCardUI.SkillData
        };
        deck.Add(playedData);

        // 1-b) 손패 리스트에서 제거 & UI 파괴
        hand.RemoveAt(slotIndex);
        Destroy(playedCardUI.gameObject);

        // 2) 잠시 대기
        yield return new WaitForSeconds(sideDelay);

        // 3) 사이드 슬롯 카드(UI)를 빈 슬롯 위치로 애니메이션
        CardUI movingCard = sideCard; 
        Vector3 startPos   = movingCard.transform.position;
        Vector3 endPos     = slotParents[slotIndex].position;
        Vector3 startScale = movingCard.transform.localScale;
        float   t          = 0f;
        
        while (t < sideAnimDuration)
        {
            t += Time.deltaTime;
            float f = Mathf.Clamp01(t / sideAnimDuration);
            movingCard.transform.position   = Vector3.Lerp(startPos, endPos, f);
            movingCard.transform.localScale = Vector3.Lerp(startScale, Vector3.one, f);
            yield return null;
        }

        // 4) 슬롯에 고정 & 드래그 가능 상태로 재초기화
        movingCard.transform.SetParent(slotParents[slotIndex], false);
        movingCard.Init(
            movingCard.MonsterData,
            movingCard.SkillData,
            playedData.cardType,
            unitSpawner,
            currentNoSpawnZones,
            currentEnemyAreaImages,
            slotParents[slotIndex],
            slotIndex,
            OnCardPlayed,
            Area,
            true,         // 드래그 가능
            Vector3.one   // 원래 크기
        );
        hand.Insert(slotIndex, movingCard);

        // 5) 다음 사이드 카드 뽑기
        DrawToSideSlot();
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}