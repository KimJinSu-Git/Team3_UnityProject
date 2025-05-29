using System.Collections.Generic;
using UnityEngine;

public enum CurrencyType
{
    Gold,
    Gem,
    // 필요한 재화 타입 추가
}

public class CurrencyIconManager : MonoBehaviour
{
    public static CurrencyIconManager Instance { get; private set; }

    [System.Serializable]
    public struct CurrencyIconEntry
    {
        public CurrencyType currencyType;
        public Sprite icon;
    }

    [Header("Currency Icons")]
    [SerializeField] private List<CurrencyIconEntry> currencyIcons;

    private Dictionary<CurrencyType, Sprite> iconDict;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            iconDict = new Dictionary<CurrencyType, Sprite>();
            foreach (var entry in currencyIcons)
            {
                if (!iconDict.ContainsKey(entry.currencyType))
                    iconDict.Add(entry.currencyType, entry.icon);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 재화 타입에 따른 아이콘 스프라이트를 반환합니다.
    /// 없으면 null 반환.
    /// </summary>
    public static Sprite GetSprite(CurrencyType currencyType)
    {
        if (Instance == null)
        {
            Debug.LogError("[CurrencyIconManager] Instance가 존재하지 않습니다!");
            return null;
        }

        if (Instance.iconDict.TryGetValue(currencyType, out var icon))
            return icon;

        Debug.LogWarning($"[CurrencyIconManager] 아이콘 없음: {currencyType}");
        return null;
    }
}