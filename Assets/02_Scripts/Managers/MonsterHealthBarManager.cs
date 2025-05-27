using UnityEngine;
using System.Collections.Generic;
using Fusion;

public class MonsterHealthBarManager : MonoBehaviour
{
    public static MonsterHealthBarManager Instance;

    [Header("HealthBar Prefabs")]
    [SerializeField] private MonsterHealthBar playerBarPrefab;
    [SerializeField] private MonsterHealthBar enemyBarPrefab;

    [Header("Common Canvas")]
    [SerializeField] private Canvas uiCanvas;               // 반드시 Render Mode = Screen Space – Overlay
    private RectTransform canvasRect;

    [Header("World→Screen Offset")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0, 2f, 0);

    private Dictionary<BaseMonsterController, MonsterHealthBar> table 
        = new Dictionary<BaseMonsterController, MonsterHealthBar>();

    private PlayerRef localRef;

    private void Awake()
    {
        Instance    = this;
        localRef    = UserManager.Instance.FusionPlayerRef;
        canvasRect  = uiCanvas.GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        Camera cam = Camera.main;
        var toRemove = new List<BaseMonsterController>();

        foreach (var kv in table)
        {
            var unit = kv.Key;
            var bar  = kv.Value;

            // (1) 유닛이 null 이거나 죽었거나, Bar 오브젝트가 이미 파괴됐다면
            if (unit == null || unit.isDead || bar == null)
            {
                // Bar가 아직 남아 있으면 파괴
                if (bar != null)
                    Destroy(bar.gameObject);

                // 나중에 Dictionary에서 제거할 키로 표시
                toRemove.Add(unit);
                continue;
            }

            // (2) 살아 있는 유닛은 기존대로 화면 위치 업데이트
            Vector3 worldPos  = unit.transform.position + worldOffset;
            Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

            bool onScreen = screenPos.z > 0
                            && screenPos.x >= 0 && screenPos.x <= Screen.width
                            && screenPos.y >= 0 && screenPos.y <= Screen.height;

            bar.gameObject.SetActive(onScreen);
            if (onScreen)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    screenPos,
                    null,
                    out Vector2 localPoint
                );
                bar.GetComponent<RectTransform>().anchoredPosition = localPoint;
            }
        }

        // (3) 표시한 키들을 한꺼번에 Dictionary에서 삭제
        foreach (var key in toRemove)
            table.Remove(key);
    }


    /// <summary>몬스터 스폰 시 호출</summary>
    public void Register(BaseMonsterController unit, float maxHp)
    {
        if (table.ContainsKey(unit)) return;

        bool isAlly = unit.PlayerRef == localRef;
        var prefab  = isAlly ? playerBarPrefab : enemyBarPrefab;

        var bar = Instantiate(prefab, uiCanvas.transform);
        bar.SetMaxHealth(maxHp);
        table.Add(unit, bar);
    }

    /// <summary>데미지 입을 때마다 호출</summary>
    public void UpdateHealth(BaseMonsterController unit, float currHp)
    {
        if (table.TryGetValue(unit, out var bar))
            bar.SetHealth(currHp);
    }

    /// <summary>몬스터 사망 시 호출</summary>
    public void Unregister(BaseMonsterController unit)
    {
        if (table.TryGetValue(unit, out var bar))
        {
            Destroy(bar.gameObject);
            table.Remove(unit);
        }
    }
}
