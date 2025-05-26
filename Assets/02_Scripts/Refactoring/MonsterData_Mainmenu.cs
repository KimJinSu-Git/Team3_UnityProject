using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData_MainMenu", menuName = "ScriptableObjects/MonsterData_MainMenu")]
public class MonsterData_Mainmenu : ScriptableObject
{
    public enum AttackType
    {
        None,
        GroundOnly,
        AirOnly,
        Both
    }

    public enum CardType
    {
        Unit,
        Spell,
        Building
    }

    public enum Rarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    [Header("🧾 Base Info")]
    public string id;                  // 유니크 카드 ID
    public string monsterName;        // 이름
    [TextArea] public string description;  // 설명

    [Header("📊 Base Stats")]
    public int baseHP;
    public float baseDamage;
    public float baseAttackSpeed;
    public float baseMoveSpeed;
    public float attackRange;
    public int cost;                  // 💰 카드 소환 비용 추가

    [Header("⚔️ Combat Type")]
    public AttackType attackType;
    public CardType cardType;
    public Rarity rarity;

    [Header("🕒 Spawn Info")]
    public int elixirCost;            // 엘릭서 소모
    public float spawnTime;           // 소환 지연 시간
    public float lifeDuration = -1f;  // 제한 시간 있는 카드 (예: 건물)

    [Header("🖼 Rendering Info")]
    public GameObject prefab;         // 인게임 프리팹
    public GameObject previewPrefab;  // 캐릭터 선택/상점 등에서 쓰는 프리뷰
    public Sprite icon;               // 카드 UI용 아이콘

    [Header("🔧 UI/Sorting")]
    public int displayOrder;          // UI 정렬 우선순위
    public string keyword;            // 검색/필터용 태그 (예: "range,ground")
}