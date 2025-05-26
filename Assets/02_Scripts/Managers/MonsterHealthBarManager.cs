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
    [SerializeField] private Canvas uiCanvas;

    [Header("World→Screen Offset")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0, 2f, 0);

    // BaseMonsterController 인스턴스 ↔ Bar 매핑
    private Dictionary<BaseMonsterController, MonsterHealthBar> table 
        = new Dictionary<BaseMonsterController, MonsterHealthBar>();

    private PlayerRef localRef;

    private void Awake()
    {
        Instance = this;
        // 로컬 플레이어 식별
        localRef = UserManager.Instance.FusionPlayerRef;
    }

    private void LateUpdate()
    {
        Camera cam = Camera.main;
        foreach (var kv in table)
        {
            var unit = kv.Key;
            var bar  = kv.Value;

            // 사망했거나 Destroy된 유닛이면 바로 제거
            if (unit == null || unit.isDead)
            {
                Destroy(bar.gameObject);
                continue;
            }

            // 월드 → 화면 좌표
            Vector3 worldPos  = unit.GameObject.transform.position + worldOffset;
            Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

            bool onScreen = screenPos.z > 0 
                && screenPos.x >= 0 && screenPos.x <= Screen.width
                && screenPos.y >= 0 && screenPos.y <= Screen.height;

            bar.gameObject.SetActive(onScreen);
            if (onScreen)
                bar.GetComponent<RectTransform>().position = screenPos;
        }
    }

    /// <summary>몬스터 스폰 시 호출</summary>
    public void Register(BaseMonsterController unit, float maxHp)
    {
        if (table.ContainsKey(unit)) return;

        // 아군인지 적군인지 판별
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
