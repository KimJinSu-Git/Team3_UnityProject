using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Unity.Mathematics;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class CardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // 외부 주입 데이터
    private MonsterData         monsterData;
    private SkillData         skillData;
    private Spawner_Network unitSpawner;
    private Collider[]        noSpawnZones; 
    private Image[]            enemyAreaImages;
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
    private Image         selfImage;
    private TMP_Text      cardNameText;
    private bool          returnedToSlot;
    // private bool hasCheckedAfford = false;
    private GameObject castingPreview;
    private GameObject castingCircleGO;
    private DrawCircleHelper circleHelper;
    
    // 스폰할 맵
    public Transform Area;
    
    // 카드 데이터 외부 조회용
    public MonsterData MonsterData => monsterData;
    public SkillData SkillData => skillData;

    private void Awake()
    {
        // 컴포넌트 캐싱
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        originalParent = transform.parent;
        originalAnchoredPos = rectTransform.anchoredPosition;
        worldCamera = Camera.main;

        // 이름 표시 텍스트 찾기
        selfImage = GetComponent<Image>();
        cardNameText = GetComponentInChildren<TMP_Text>();
    }

    private void Update()
    {
        int cost = monsterData != null ? monsterData.cost : skillData != null ? skillData.cost : 0;
        if (ElixirManager.Instance.GetCurrentElixir() >= cost)
        {
            isDraggable = true;
            SetVisualState(true);
        }
        else
        {
            isDraggable = false;
            SetVisualState(false);
        }
        
        // if (ElixirManager.Instance.GetCurrentElixir() >= monsterData.cost /*&& isDraggable*/)
        // {
        //     isDraggable = true;
        //     SetVisualState(true);
        // }
        // else 
        // {
        //     isDraggable = false;
        //     SetVisualState(false);
        // }
    }
    private void SetVisualState(bool colorOn)
    {
        // Color.white = 원색, Color.gray = 흑백 느낌
        var c = colorOn ? Color.white : Color.gray;

        if (selfImage   != null) selfImage.color   = c;
        if (cardNameText!= null) cardNameText.color = c;
    }

    /// 카드 초기화: 데이터, 부모 슬롯, 드래그 설정, 스케일, 콜백 등
    public void Init(
        MonsterData monsterData,
        SkillData skillData,
        Spawner_Network spawner,
        Collider[] noSpawnZones,
        Image[] areaImages,
        Transform parentSlot,
        int index,
        Action<int> onCardPlayed,
        Transform Area,
        bool draggable = true,
        Vector3? startScale = null
    )
    {
        // 데이터 & 콜백 할당
        this.monsterData = monsterData;
        this.skillData = skillData;
        unitSpawner = spawner;
        this.noSpawnZones = noSpawnZones;
        this.enemyAreaImages = areaImages;
        slotIndex = index;
        this.onCardPlayed = onCardPlayed;
        this.Area = Area;
        isDraggable = draggable;

        // 슬롯 부모에 붙이고 위치·크기 초기화
        transform.SetParent(parentSlot, false);
        rectTransform = GetComponent<RectTransform>();
        originalParent = parentSlot;
        originalAnchoredPos = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition = Vector2.zero;
        transform.localScale = startScale ?? Vector3.one;

        // 카드 이름 표시
        if (monsterData != null) cardNameText.text = monsterData.monsterName;
        if (skillData != null) cardNameText.text = skillData.skillName;
    }

    private bool IsInNoSpawnZone(Vector3 point)
    {
        foreach (var zone in noSpawnZones)
        {
            if (zone != null && zone.bounds.Contains(point))
                return true;
        }
        return false;
    }
    
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        // 드래그 시작: 투명화, 프리뷰 생성
        returnedToSlot = false;
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        
        if (monsterData != null)
            previewInstance = Instantiate(monsterData.previewPrefab);
        
        transform.SetParent(transform.root, false);
        foreach (var img in enemyAreaImages)
            img.enabled = true;
        
        if (skillData != null)
        {
            castingCircleGO = new GameObject("CastingCircle");
            circleHelper = castingCircleGO.AddComponent<DrawCircleHelper>();
            circleHelper.Draw(skillData.range);
        }
        
        if (skillData != null && skillData.castingCircle != null)
        {
            castingPreview = new GameObject("CastingCircle");
            SpriteRenderer renderer = castingPreview.AddComponent<SpriteRenderer>();
            renderer.sprite = skillData.castingCircle;
            renderer.sortingOrder = 100;

            float range = skillData.range;
            castingPreview.transform.localScale = new Vector3(range * 2f, 1f, range * 2f);
        }
        
        // previewInstance = Instantiate(monsterData.previewPrefab);
        // transform.SetParent(transform.root, false);
        // for (int i = 0; i < enemyAreaImages.Length; i++)
        // {
        //     enemyAreaImages[i].enabled = true;
        // }
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

        if (Physics.Raycast(worldCamera.ScreenPointToRay(eventData.position), out var hit, 100f)
            && !IsInNoSpawnZone(hit.point))
        {
            if (previewInstance != null)
                previewInstance.transform.position = hit.point;

            if (castingCircleGO != null)
                castingCircleGO.transform.position = hit.point + Vector3.up * 0.1f;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        // 드래그 종료: 원상 복구 및 스폰/콜백 실행
        foreach (var img in enemyAreaImages)
            img.enabled = false;

        returnedToSlot = eventData.pointerEnter == originalParent.gameObject;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        transform.SetParent(originalParent, false);
        rectTransform.anchoredPosition = Vector2.zero;
        
        if (previewInstance != null) Destroy(previewInstance);
        if (returnedToSlot) return;

        if (Physics.Raycast(worldCamera.ScreenPointToRay(eventData.position), out var hit, 100f)
            && !IsInNoSpawnZone(hit.point))
        {
            Vector3 worldSpawnPos = hit.point;

            if (monsterData != null)
            {
                ElixirManager.Instance.UseElixir(monsterData.cost);
                unitSpawner.RequestSpawn(monsterData.name, worldSpawnPos, Quaternion.identity);
            }
            else if (skillData != null)
            {
                ElixirManager.Instance.UseElixir(skillData.cost);
                SkillManager.Instance.CastSkill(skillData, worldSpawnPos);
            }

            onCardPlayed?.Invoke(slotIndex);
            Destroy(gameObject);
        }
        
        if(castingCircleGO != null)
            Destroy(castingCircleGO);
        
        if (castingPreview != null)
            Destroy(castingPreview);
        
        // for (int i = 0; i < enemyAreaImages.Length; i++)
        // {
        //     enemyAreaImages[i].enabled = false;
        // }
        // returnedToSlot = eventData.pointerEnter == originalParent.gameObject;
        // canvasGroup.alpha = 1f;
        // canvasGroup.blocksRaycasts = true;
        // transform.SetParent(originalParent, false);
        // rectTransform.anchoredPosition = Vector2.zero;
        //
        // if (previewInstance != null) Destroy(previewInstance);
        // if (returnedToSlot) return;
        //
        // if (Physics.Raycast(worldCamera.ScreenPointToRay(eventData.position), out var hit, 100f)
        //     && !IsInNoSpawnZone(hit.point))
        // {
        //     ElixirManager.Instance.UseElixir(monsterData.cost);
        //     //Vector3 localSpawnPos = Area.InverseTransformPoint(hit.point);
        //     Vector3 worldSpawnPos = hit.point;
        //     // if (UserManager.Instance.FusionPlayerRef == SessionManager.Instance.CurrentGameRoomInfo.ClientPlayer)
        //     // {
        //     //     unitSpawner.RequestSpawn(monsterData.name, -localSpawnPos, quaternion.identity);
        //     // }
        //     // else
        //     // {
        //     //     unitSpawner.RequestSpawn(monsterData.name, localSpawnPos, quaternion.identity);
        //     // }
        //     unitSpawner.RequestSpawn(monsterData.name, worldSpawnPos, quaternion.identity);
        //
        //     onCardPlayed?.Invoke(slotIndex);
        //     Destroy(gameObject);
        // }
    }
}
