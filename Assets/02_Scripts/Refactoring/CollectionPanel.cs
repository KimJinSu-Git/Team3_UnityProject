using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollectionPanel : MonoBehaviour
{
    public Transform collectionGrid;
    public GameObject slotPrefab;

    public DeckManagerUI deckManagerUI; // 덱 UI 매니저 참조 (필요 시)
    
    public List<BaseData> allCards; // 모든 카드 리스트 (에디터에서 세팅)

    void Start()
    {
        // 덱 준비 완료 이벤트 구독
        DeckManager.Instance.OnCurrentDeckReady += Refresh;

        // 혹시 이미 덱이 준비되어 있으면 즉시 갱신
        if (DeckManager.Instance.currentPlayerDeck.Count > 0)
        {
            Refresh();
        }
    }

    private void OnDestroy()
    {
        // 이벤트 해제 (메모리 누수 방지)
        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.OnCurrentDeckReady -= Refresh;
        }
    }

    public void Refresh()
    {
        // 기존 UI 클리어
        foreach (Transform child in collectionGrid)
            Destroy(child.gameObject);

        // 현재 덱에 포함된 카드 ID 집합 생성
        var deckCardIds = new HashSet<string>(DeckManager.Instance.currentPlayerDeck.Select(card => card.id));

        foreach (var card in allCards)
        {
            if (deckCardIds.Contains(card.id))
                continue;  // 이미 덱에 포함된 카드는 컬렉션에서 제외

            var slot = Instantiate(slotPrefab, collectionGrid).GetComponent<SlotUI>();
            slot.Setup(card, -1, deckManagerUI, SlotMode.Collection);
        }
    }
}