using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Spawner & Prefabs")]
    public PlayerUnitSpawner unitSpawner;  // 실제 스폰용
    public GameObject ghostPrefab;     // 미리보기 고스트 프리팹
    
    
    [Header("Enemy Area")]
    public Collider enemyAreaCollider;
    
    
    [Header("Enemy Area")]
    public Image enemyeAreaImage;
    
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector2 originalAnchoredPos;
    private Transform originalParent;
    private GameObject ghostInstance;
    private Camera worldCamera;

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
        
        enemyeAreaImage.enabled = true;
        
       
        ghostInstance = Instantiate(ghostPrefab);

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

      
        if (ghostInstance != null)
        {
            Ray ray = worldCamera.ScreenPointToRay(eventData.position);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                Vector3 spawnPos = hit.point;
                if (enemyAreaCollider.bounds.Contains(spawnPos))
                {
                    return;
                }
                ghostInstance.transform.position = hit.point;
            }
              
        }
        
        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        enemyeAreaImage.enabled = false;
        
        
        if (eventData.pointerEnter == originalParent.gameObject)
            returnedToSlot = true;

    
        canvasGroup.alpha          = 1f;
        canvasGroup.blocksRaycasts = true;
        transform.SetParent(originalParent, true);
        rectTransform.anchoredPosition = originalAnchoredPos;

 
        if (ghostInstance != null)
            Destroy(ghostInstance);

    
        if (returnedToSlot)
            return;

      
        Ray ray = worldCamera.ScreenPointToRay(eventData.position);
        
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
           
            Vector3 spawnPos = hit.point;
            
            if (enemyAreaCollider.bounds.Contains(spawnPos))
            {
                return;
            }
            

            unitSpawner.SpawnAt(spawnPos);
            Destroy(gameObject);
        }
    }
}

