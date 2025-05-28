using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class CardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // 🔹 외부 주입 데이터
    // private MonsterData monsterData;
    // private SkillData skillData;
    private BaseData baseData;
    private Spawner_Network unitSpawner;
    private Collider[] noSpawnZones; 
    private Image[] enemyAreaImages;
    private int slotIndex;
    private Action<int> onCardPlayed;
    private bool isDraggable;
    //private CardDataWrapper.CardType cardType;

    // 🔹 UI 요소들
    [SerializeField] private Image iconImage;         // 카드 아이콘 이미지 (👈 추가)
    [SerializeField] private TMP_Text cardNameText;   // 카드 이름 텍스트

    // 🔹 내부 상태
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector2 originalAnchoredPos;
    private Transform originalParent;
    private Camera worldCamera;
    private GameObject previewInstance;
    private GameObject castingPreview;
    private GameObject castingCircleGO;
    private DrawCircleHelper circleHelper;
    public Transform Area;

    // 🔹 외부 조회용
    public BaseData BaseData => baseData;
    // public MonsterData MonsterData => monsterData;
    // public SkillData SkillData => skillData;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        originalParent = transform.parent;
        originalAnchoredPos = rectTransform.anchoredPosition;
        worldCamera = Camera.main;
    }

    private void Update()
    {
        //int cost = monsterData != null ? monsterData.cost : skillData != null ? skillData.cost : 0;
        int cost = baseData.cost;
        bool affordable = ElixirManager.Instance.GetCurrentElixir() >= cost;
        isDraggable = affordable;
        SetVisualState(affordable);
    }

    private void SetVisualState(bool active)
    {
        Color c = active ? Color.white : Color.gray;
        if (iconImage != null) iconImage.color = c;
        if (cardNameText != null) cardNameText.color = c;
    }

    public void Init(
        BaseData data,
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
        // this.monsterData = monsterData;
        // this.skillData = skillData;
        this.baseData = data;
        unitSpawner = spawner;
        this.noSpawnZones = noSpawnZones;
        this.enemyAreaImages = areaImages;
        slotIndex = index;
        this.onCardPlayed = onCardPlayed;
        this.Area = Area;
        isDraggable = draggable;

        transform.SetParent(parentSlot, false);
        rectTransform = GetComponent<RectTransform>();
        originalParent = parentSlot;
        originalAnchoredPos = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition = Vector2.zero;
        transform.localScale = startScale ?? Vector3.one;

        // 🔹 이름 및 아이콘 적용
        if (data != null)
        {
            if(cardNameText != null) cardNameText.text = data.name;
            if(iconImage != null) iconImage.sprite = data.icon;
        }
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

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        if (baseData.cardDataType == CardDataType.Monster)
        {
            if (baseData is MonsterData monsterData)
            {
                previewInstance = Instantiate(monsterData.previewPrefab);
                foreach (var img in enemyAreaImages) img.enabled = true;
                foreach (var zone in noSpawnZones) zone.enabled = true;
            }
        }
        else if (baseData.cardDataType == CardDataType.Skill)
        {
            if (baseData is SkillData skillData)
            {
                foreach (var zone in noSpawnZones) zone.enabled = false;

                castingCircleGO = new GameObject("CastingCircle");
                circleHelper = castingCircleGO.AddComponent<DrawCircleHelper>();
                circleHelper.Draw(skillData.range);

                if (skillData.castingCircle != null)
                {
                    castingPreview = new GameObject("CastingCircleSprite");
                    var renderer = castingPreview.AddComponent<SpriteRenderer>();
                    renderer.sprite = skillData.castingCircle;
                    renderer.sortingOrder = 100;

                    float r = skillData.range;
                    castingPreview.transform.localScale = new Vector3(r * 2, 1f, r * 2);
                }
            }
        }

        transform.SetParent(transform.root, false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.root as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );
        rectTransform.localPosition = localPoint;

        if (Physics.Raycast(worldCamera.ScreenPointToRay(eventData.position), out var hit, 100f))
        {
            bool canPreview =
                (baseData != null && !IsInNoSpawnZone(hit.point));

            if (canPreview)
            {
                if (previewInstance != null)
                    previewInstance.transform.position = hit.point;

                if (castingCircleGO != null)
                    castingCircleGO.transform.position = hit.point + Vector3.up * 0.1f;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        foreach (var img in enemyAreaImages) img.enabled = false;
        foreach (var zone in noSpawnZones) zone.enabled = true;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        transform.SetParent(originalParent, false);
        rectTransform.anchoredPosition = Vector2.zero;

        if (previewInstance != null) Destroy(previewInstance);
        if (castingCircleGO != null) Destroy(castingCircleGO);
        if (castingPreview != null) Destroy(castingPreview);

        if (eventData.pointerEnter == originalParent.gameObject)
            return;

        if (Physics.Raycast(worldCamera.ScreenPointToRay(eventData.position), out var hit, 100f))
        {
            Vector3 worldSpawnPos = hit.point;

            if (baseData.cardDataType == CardDataType.Monster)
            {
                if (IsInNoSpawnZone(worldSpawnPos)) return;

                ElixirManager.Instance.UseElixir(baseData.cost);
                unitSpawner.RequestSpawn(baseData.name, worldSpawnPos, Quaternion.identity);
            }
            else if (baseData.cardDataType == CardDataType.Skill)
            {
                ElixirManager.Instance.UseElixir(baseData.cost);
                SkillManager.Instance.CastSkill(baseData.name, worldSpawnPos);
            }
            Debug.Log($"[CardUI] OnEndDrag: slotIndex={slotIndex}, invoking OnCardPlayed");
            onCardPlayed?.Invoke(slotIndex);
        }
    }
}
