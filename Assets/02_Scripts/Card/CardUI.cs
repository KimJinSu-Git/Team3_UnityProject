using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class CardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // 외부 주입 데이터
    private CardData          cardData;
    private PlayerUnitSpawner unitSpawner;
    private Collider          enemyAreaCollider;
    private Image             enemyAreaImage;
    private int               slotIndex;
    private Action<int>       onCardPlayed;
    private bool              isDraggable;

    // 내부 상태
    private CanvasGroup   canvasGroup;
    private RectTransform rectTransform;
    private Vector2       originalAnchoredPos;
    private Transform     originalParent;
    private Camera        worldCamera;
    private GameObject    previewInstance;
    private TMP_Text      cardNameText;
    private bool          returnedToSlot;

    // 카드 데이터 외부 조회용
    public CardData CardData => cardData;

    void Awake()
    {
        // 컴포넌트 캐싱
        canvasGroup        = GetComponent<CanvasGroup>();
        rectTransform      = GetComponent<RectTransform>();
        originalParent     = transform.parent;
        originalAnchoredPos = rectTransform.anchoredPosition;
        worldCamera        = Camera.main;

        // 이름 표시 텍스트 찾기
        cardNameText = GetComponentInChildren<TMP_Text>();
    }


    /// 카드 초기화: 데이터, 부모 슬롯, 드래그 설정, 스케일, 콜백 등
    public void Init(
        CardData data,
        PlayerUnitSpawner spawner,
        Collider areaCollider,
        Image areaImage,
        Transform parentSlot,
        int index,
        Action<int> onCardPlayed,
        bool draggable = true,
        Vector3? startScale = null
    )
    {
        // 데이터 & 콜백 할당
        cardData          = data;
        unitSpawner       = spawner;
        enemyAreaCollider = areaCollider;
        enemyAreaImage    = areaImage;
        slotIndex         = index;
        this.onCardPlayed = onCardPlayed;
        isDraggable       = draggable;

        // 슬롯 부모에 붙이고 위치·크기 초기화
        transform.SetParent(parentSlot, false);
        rectTransform         = GetComponent<RectTransform>();
        originalParent        = parentSlot;
        originalAnchoredPos   = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition = Vector2.zero;
        transform.localScale  = startScale ?? Vector3.one;

        // 카드 이름 표시
        cardNameText.text = data.unitName;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        // 드래그 시작: 투명화, 프리뷰 생성
        returnedToSlot = false;
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        enemyAreaImage.enabled = true;
        previewInstance = Instantiate(cardData.previewPrefab);
        transform.SetParent(transform.root, false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        // 드래그 중: UI 위치 및 월드 프리뷰 갱신
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.root as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );
        rectTransform.localPosition = localPoint;

        if (Physics.Raycast(worldCamera.ScreenPointToRay(eventData.position), out var hit, 100f) &&
            !enemyAreaCollider.bounds.Contains(hit.point))
        {
            previewInstance.transform.position = hit.point;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        // 드래그 종료: 원상 복구 및 스폰/콜백 실행
        enemyAreaImage.enabled = false;
        returnedToSlot = eventData.pointerEnter == originalParent.gameObject;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        transform.SetParent(originalParent, false);
        rectTransform.anchoredPosition = originalAnchoredPos;

        if (previewInstance != null) Destroy(previewInstance);
        if (returnedToSlot) return;

        if (Physics.Raycast(worldCamera.ScreenPointToRay(eventData.position), out var hit, 100f) &&
            !enemyAreaCollider.bounds.Contains(hit.point))
        {
            unitSpawner.SpawnAt(cardData, hit.point);
            onCardPlayed?.Invoke(slotIndex);
            Destroy(gameObject);
        }
    }
}
