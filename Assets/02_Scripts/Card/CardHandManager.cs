using System;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CardHandManager : MonoBehaviour
{
    public static  CardHandManager Instance;

    [Header("덱 설정")]
    // public List<MonsterData> fullDeck;        // 전체 카드 데이터(8장)
    public List<CardDataWrapper> fullDeck; // Monster or Skill 카드 모두 포함
    private List<CardDataWrapper> deck;          // 런타임용 덱
    public Dictionary<string, MonsterData> monsterDatas; // 스텟등 확정성을 위해서 MonsterData전부를 넘겨줌

    
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

    // private List<MonsterData> deck;          // 런타임용 덱
    private List<CardUI>   hand  = new List<CardUI>(); // 현재 손패
    private CardUI         sideCard;      // 사이드 슬롯 카드

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
        
        // foreach (var zone in hostEnemyZones)
        //     zone.gameObject.SetActive(false);
        // foreach (var zone in clientEnemyZones)
        //     zone.gameObject.SetActive(false);
        // foreach (var zone in currentNoSpawnZones)
        //     zone.gameObject.SetActive(false);
        foreach (var img in currentEnemyAreaImages)
            img.enabled = false;
        
        // 1) 덱 복사 후 셔플
        // deck = new List<MonsterData>(fullDeck);
        deck = new List<CardDataWrapper>(fullDeck);
        Shuffle(deck);
        // 몬스터 데이터 캐싱
        monsterDatas = new Dictionary<string, MonsterData>();
        foreach (var data in deck)
        {
            if (data.IsMonster && !monsterDatas.ContainsKey(data.monsterData.name))
            {
                monsterDatas.Add(data.monsterData.name, data.monsterData);
            }
        }
        // foreach (MonsterData monsterData in deck)
        // {
        //     monsterDatas.Add(monsterData.name, monsterData);
        // }
        // 2) 초기 손패 4장 뽑기
        for (int i = 0; i < slotParents.Length; i++)
            DrawToSlot(i);

        // 3) 사이드 슬롯에 5번째 카드 배치
        DrawToSideSlot();
    }
    
    private void DrawToSlot(int slotIndex)   // 지정된 슬롯에 덱 맨 앞 카드를 뽑아서 UI 생성
    {
        if (deck.Count == 0) return;

        // 덱에서 카드 데이터 꺼내기
        // MonsterData data = deck[0];
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
        // card.Init(
        //     data.monsterData,
        //     data.skillData,
        //     unitSpawner,
        //     noSpawnZones,
        //     enemyAreaImages,
        //     slotParents[slotIndex], // 부모 슬롯 지정
        //     slotIndex,
        //     OnCardPlayed,
        //     Area,
        //     true,                    // 드래그 가능
        //     Vector3.one              // 기본 크기
        // );
        hand.Insert(slotIndex, card);
    }
    
    private void DrawToSideSlot()    // 사이드 슬롯에 카드 하나 배치 (드래그 불가, 작은 크기)
    {
        if (deck.Count == 0) return;

        // 덱에서 카드 데이터 꺼내기
        // MonsterData data = deck[0];
        CardDataWrapper data = deck[0];
        deck.RemoveAt(0);

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
        // sideCard.Init(
        //     data.monsterData,
        //     data.skillData,
        //     unitSpawner,
        //     noSpawnZones,
        //     enemyAreaImages,
        //     sideSlotParent,
        //     -1,               // 슬롯 인덱스 없음
        //     null,             // 콜백 없음
        //     Area,
        //     false,            // 드래그 불가
        //     sideScale         // 축소된 크기
        // );
    }
    
    private void OnCardPlayed(int slotIndex)    // 카드 유닛 배치 시 호출되는 콜백
    {
        // 3) 사이드 슬롯 카드 → 빈 슬롯으로 이동
        StartCoroutine(MoveSideToHand(slotIndex));
    }
    
    private IEnumerator MoveSideToHand(int slotIndex)   // 사이드 슬롯 카드를 빈 슬롯으로 이동시키고 재초기화
    {
        // 1) 대기
        yield return new WaitForSeconds(sideDelay);

        // 1) 사용된 카드 데이터를 덱 뒤로 이동
        // var playedData = hand[slotIndex].MonsterData;
        // deck.Add(playedData);

        // var playedData = new CardDataWrapper
        // {
        //     monsterData = hand[slotIndex].MonsterData,
        //     skillData = hand[slotIndex].SkillData
        // };
        CardDataWrapper playedData = new CardDataWrapper
        {
            cardType = sideCard.MonsterData != null ? CardDataWrapper.CardType.Monster : CardDataWrapper.CardType.Skill,
            monsterData = sideCard.MonsterData,
            skillData = sideCard.SkillData
        };
        deck.Add(playedData);
        
        // 2) 손패에서 카드 UI 제거
        hand.RemoveAt(slotIndex);
        hand.Insert(slotIndex, sideCard);
        
        
        // 2) 위치 & 스케일 애니메이션
        Vector3 startPos   = sideCard.transform.position;
        Vector3 endPos     = slotParents[slotIndex].position;
        Vector3 startScale = sideCard.transform.localScale;
        float   t          = 0f;
        while (t < sideAnimDuration)
        {
            t += Time.deltaTime;
            float f = Mathf.Clamp01(t / sideAnimDuration);
            sideCard.transform.position   = Vector3.Lerp(startPos, endPos, f);
            sideCard.transform.localScale = Vector3.Lerp(startScale, Vector3.one, f);
            yield return null;
        }

        // 3) 빈 슬롯으로 부모 변경
        sideCard.transform.SetParent(slotParents[slotIndex], false);

        // 4) 다시 손패 카드로 재초기화 (드래그 가능, 콜백 설정)
        sideCard.Init(sideCard.MonsterData, sideCard.SkillData, playedData.cardType,
            unitSpawner, currentNoSpawnZones, currentEnemyAreaImages,
            slotParents[slotIndex], slotIndex, OnCardPlayed, Area,
            true, Vector3.one);
        // sideCard.Init(
        //     sideCard.MonsterData,
        //     sideCard.SkillData,
        //     unitSpawner,
        //     noSpawnZones,
        //     enemyAreaImages,
        //     slotParents[slotIndex],
        //     slotIndex,
        //     OnCardPlayed,
        //     Area,
        //     true,
        //     Vector3.one
        // );
        

        // 6) 다음 사이드 카드 뽑기
        DrawToSideSlot();
    }
    
    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j    = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
