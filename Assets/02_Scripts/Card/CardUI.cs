using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class CardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // CardHandManager에서 Init()으로 주입
    private CardData             cardData;
    private PlayerUnitSpawner    unitSpawner;
    private Collider             enemyAreaCollider;
    private Image                enemyAreaImage;
    
    public CardData CardData => cardData;
    
    // 드래그용 내부 필드
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector2 originalAnchoredPos;
    private Transform originalParent;
    private Camera worldCamera;
    private GameObject previewInstance;
    private bool returnedToSlot;

    
    public void Init(CardData data, PlayerUnitSpawner spawner, Collider areaCollider, Image areaImage, Transform parentSlot)
    {
        cardData          = data;
        unitSpawner       = spawner;
        enemyAreaCollider = areaCollider;
        enemyAreaImage    = areaImage;

        // 부모 슬롯(레이아웃) 지정
        originalParent     = parentSlot;
        transform.SetParent(parentSlot, false);
    }
    
    void Awake()
    {
        canvasGroup        = GetComponent<CanvasGroup>();
        rectTransform      = GetComponent<RectTransform>();
        originalParent     = transform.parent;
        originalAnchoredPos = rectTransform.anchoredPosition;
        worldCamera = Camera.main;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        returnedToSlot = false;
        canvasGroup.alpha          = 0f;
        canvasGroup.blocksRaycasts = false;
        enemyAreaImage.enabled     = true;

        if (cardData?.previewPrefab != null)
            previewInstance = Instantiate(cardData.previewPrefab);
        else
            Debug.LogWarning("Preview Prefab 누락");

        transform.SetParent(transform.root, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.root as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );
        rectTransform.localPosition = localPoint;

      
        if (previewInstance != null)
        {
            Ray ray = worldCamera.ScreenPointToRay(eventData.position);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                Vector3 spawnPos = hit.point;
                if (enemyAreaCollider.bounds.Contains(spawnPos))
                {
                    return;
                }
                previewInstance.transform.position = hit.point;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        enemyAreaImage.enabled = false;
        
        if (eventData.pointerEnter == originalParent.gameObject)
            returnedToSlot = true;
        
        canvasGroup.alpha          = 1f;
        canvasGroup.blocksRaycasts = true;
        transform.SetParent(originalParent, true);
        rectTransform.anchoredPosition = originalAnchoredPos;
        
        if (previewInstance != null)
            Destroy(previewInstance);
        
        if (returnedToSlot)
            return;
        
        if (Physics.Raycast(worldCamera.ScreenPointToRay(eventData.position), out RaycastHit hit, Mathf.Infinity))
        {
            if (!enemyAreaCollider.bounds.Contains(hit.point))
            {
                unitSpawner.SpawnAt(cardData, hit.point);
                Destroy(gameObject);
            }
        }
    }
}

