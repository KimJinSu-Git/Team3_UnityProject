using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardHandManager : MonoBehaviour
{
    [Header("덱 설정")]
    public List<MonsterData> fullDeck;        // 전체 카드 데이터(8장)

    [Header("UI 슬롯 & 프리팹")]
    public CardUI      cardPrefab;         // 카드 UI 프리팹
    public Transform[] slotParents;        // 손패 슬롯 부모(4개)
    public Transform   sideSlotParent;     // 사이드 슬롯 부모(5번째)

    [Header("사이드 슬롯 스케일 & 타이밍")]
    public Vector3     sideScale        = new Vector3(0.5f, 0.5f, 1f);
    public float       sideDelay        = 2f;   // 사이드→손패 전환 대기 시간
    public float       sideAnimDuration = 0.5f; // 이동 애니메이션 시간

    [Header("스포너 & 적 영역")]
    public PlayerUnitSpawner unitSpawner;
    public Collider          enemyAreaCollider;
    public Image             enemyAreaImage;

    private List<MonsterData> deck;          // 런타임용 덱
    private List<CardUI>   hand  = new List<CardUI>(); // 현재 손패
    private CardUI         sideCard;      // 사이드 슬롯 카드

    void Start()
    {
        // 1) 덱 복사 후 셔플
        deck = new List<MonsterData>(fullDeck);
        Shuffle(deck);

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
        var data = deck[0];
        deck.RemoveAt(0);

        // 카드 인스턴스 생성 및 초기화
        var card = Instantiate(cardPrefab);
        card.Init(
            data,
            unitSpawner,
            enemyAreaCollider,
            enemyAreaImage,
            slotParents[slotIndex], // 부모 슬롯 지정
            slotIndex,
            OnCardPlayed,
            true,                    // 드래그 가능
            Vector3.one              // 기본 크기
        );
        hand.Insert(slotIndex, card);
    }
    
    private void DrawToSideSlot()    // 사이드 슬롯에 카드 하나 배치 (드래그 불가, 작은 크기)
    {
        if (deck.Count == 0) return;

        // 덱에서 카드 데이터 꺼내기
        var data = deck[0];
        deck.RemoveAt(0);

        // 기존 사이드 카드가 있으면 삭제 (사이드 슬롯에 남아 있는 경우만)
        if (sideCard != null && sideCard.transform.parent == sideSlotParent)
            Destroy(sideCard.gameObject);

        // 새 사이드 카드 인스턴스 생성 및 초기화
        sideCard = Instantiate(cardPrefab);
        sideCard.Init(
            data,
            unitSpawner,
            enemyAreaCollider,
            enemyAreaImage,
            sideSlotParent,
            -1,               // 슬롯 인덱스 없음
            null,             // 콜백 없음
            false,            // 드래그 불가
            sideScale         // 축소된 크기
        );
    }
    
    private void OnCardPlayed(int slotIndex)    // 카드 유닛 배치 시 호출되는 콜백
    {
        // 1) 사용된 카드 데이터를 덱 뒤로 이동
        var playedData = hand[slotIndex].MonsterData;
        deck.Add(playedData);

        // 2) 손패에서 카드 UI 제거
        Destroy(hand[slotIndex].gameObject);
        hand.RemoveAt(slotIndex);

        // 3) 사이드 슬롯 카드 → 빈 슬롯으로 이동
        StartCoroutine(MoveSideToHand(slotIndex));
    }
    
    private IEnumerator MoveSideToHand(int slotIndex)   // 사이드 슬롯 카드를 빈 슬롯으로 이동시키고 재초기화
    {
        // 1) 대기
        yield return new WaitForSeconds(sideDelay);

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
        sideCard.Init(
            sideCard.MonsterData,
            unitSpawner,
            enemyAreaCollider,
            enemyAreaImage,
            slotParents[slotIndex],
            slotIndex,
            OnCardPlayed,
            true,
            Vector3.one
        );

        // 5) 손패 리스트에 삽입
        hand.Insert(slotIndex, sideCard);

        // 6) 다음 사이드 카드 뽑기
        DrawToSideSlot();
    }
    
    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j    = Random.Range(0, i + 1);
            T tmp    = list[i];
            list[i]  = list[j];
            list[j]  = tmp;
        }
    }
}
