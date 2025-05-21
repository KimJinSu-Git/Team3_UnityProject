using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class CardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("CardData")]
    public CardData cardData;  
    
    [Header("Spawner & Prefabs")]
    public PlayerUnitSpawner unitSpawner; 
    
    [Header("Enemy Area")]
    public Collider enemyAreaCollider;
    public Image enemyAreaImage;
    
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector2 originalAnchoredPos;
    private Transform originalParent;
    private Camera worldCamera;
    private GameObject previewInstance;
    private bool returnedToSlot;

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
            Debug.LogWarning("Preview Prefab이 할당되지 않았습니다.");

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
               // Destroy(gameObject);
            }
        }
    }
}

