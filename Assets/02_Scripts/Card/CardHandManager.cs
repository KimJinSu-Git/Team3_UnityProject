using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CardHandManager : MonoBehaviour
{
    [Header("덱 설정")]
    public List<CardData> fullDeck;        // 인스펙터에 7장 이상 세팅

    [Header("UI 슬롯 & 프리팹")]
    public CardUI      cardPrefab;         // Prefab: 위 CardUI 컴포넌트를 가진 오브젝트
    public Transform[] slotParents;        // 4개의 슬롯 Transform 배열

    [Header("스포너 & 적 영역")]
    public PlayerUnitSpawner unitSpawner;
    public Collider          enemyAreaCollider;
    public Image             enemyAreaImage;

    private List<CardData> deck;           // 런타임용 덱
    private List<CardUI>   hand = new List<CardUI>();  // 현재 4장 손패

    void Start()
    {
        // 덱 복사 및 셔플(원하면)
        deck = new List<CardData>(fullDeck);
        // Shuffle(deck);

        // 손패 초기 생성
        for (int i = 0; i < 4; i++)
            DrawToSlot(i);
    }

    void DrawToSlot(int slotIndex)
    {
        if (deck.Count == 0) { Debug.LogWarning("덱이 비었습니다"); return; }
        // 덱 맨 앞에서 뽑기
        var data = deck[0];
        deck.RemoveAt(0);

        // 카드 UI 생성
        var card = Instantiate(cardPrefab, slotParents[slotIndex]);
        card.Init(
            data,
            unitSpawner,
            enemyAreaCollider,
            enemyAreaImage,
            slotParents[slotIndex]
        );
        // 클릭(드래그) 후 스폰→보충 로직 연결
        // CardUI 스크립트 내에서 직접 스폰하므로 별도 콜백 불필요

        hand.Insert(slotIndex, card);
    }

    // 카드 사용(드래그 끝) 후 _자동_으로 호출되는 콜백은 없으므로,
    // CardUI.OnEndDrag 에서 스폰만 처리하고, 아래 메서드를 수동으로 호출해도 됩니다.
    public void OnCardPlayed(int slotIndex)
    {
        // 1) 플레이된 카드를 덱 맨 뒤로
        var playedData = hand[slotIndex].CardData;
        deck.Add(playedData);

        // 2) 사용된 카드 UI 제거
        Destroy(hand[slotIndex].gameObject);
        hand.RemoveAt(slotIndex);

        // 3) 슬롯 0~2 위치에 있는 카드들을 한 칸씩 앞으로 이동시키기
        for (int i = slotIndex; i < hand.Count; i++)
        {
            hand[i].transform.SetParent(slotParents[i], false);
        }

        // 4) 덱에서 다음 카드 뽑아 3번 슬롯(마지막) 채우기
        DrawToSlot(3);
    }
}

