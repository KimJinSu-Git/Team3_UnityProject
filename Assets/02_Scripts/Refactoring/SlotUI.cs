using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public enum SlotMode { Deck, Collection }

public class SlotUI : MonoBehaviour
{
    public static readonly Dictionary<string, string> koreanNameMap = new Dictionary<string, string>()
    {
        { "Warrior", "워리어" },
        { "Golem", "골렘" },
        { "Mage", "메이지" },
        { "ArrowRain", "화살비" },
        { "Rogue", "로그" },
        { "Minion", "미니언" },
        { "Necromancer", "네크로맨서" },
        { "Cannon", "캐논" },
        { "Fireball", "화염구" }
    };

    [Header("공용")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text progressText;

    [Header("버튼")]
    [SerializeField] private Button actionButton;
    [SerializeField] private TMP_Text actionButtonText;
    [SerializeField] private Button infoButton;
    [SerializeField] private GameObject infoPanel;

    private DeckManagerUI deckManager;
    private BaseData cardData;
    private int slotIndex;
    private SlotMode mode;

    private void Awake()
    {
        if (infoButton != null)
            infoButton.onClick.AddListener(ToggleInfoPanel);

        if (infoPanel != null)
            infoPanel.SetActive(false);
    }

    public void Setup(BaseData card, int index, DeckManagerUI manager, SlotMode slotMode)
    {
        cardData = card;
        slotIndex = index;
        deckManager = manager;
        mode = slotMode;

        if (cardData == null)
        {
            Debug.LogError("[SlotUI] Setup 호출 시 cardData가 null입니다.");
            return;
        }

        if (cardData is MonsterData monster)
        {
            SetupMonsterData(monster);
        }
        else if (cardData is SkillData skill)
        {
            SetupSkillData(skill);
        }
        else
        {
            Debug.LogError("[SlotUI] 알 수 없는 카드 타입입니다.");
            ClearUI();
        }

        SetupButtonEvents();
    }

    private void SetupMonsterData(MonsterData monster)
    {
        icon.sprite = monster.icon ?? GetDefaultIcon();
        nameText.text = GetKoreanName(monster.cardName);
        levelText.text = $"Lv.{monster.level}";
        costText.text = monster.cost.ToString();
        progressText.text = ""; // 필요하면 채우기
    }

    private void SetupSkillData(SkillData skill)
    {
        icon.sprite = skill.icon ?? GetDefaultIcon();
        nameText.text = GetKoreanName(skill.cardName);
        costText.text = skill.cost.ToString();
        progressText.text = ""; // 필요하면 채우기
    }

    private string GetKoreanName(string engName)
    {
        if (string.IsNullOrEmpty(engName))
            return "Unknown";

        return koreanNameMap.TryGetValue(engName, out var korName) ? korName : engName;
    }

    private Sprite GetDefaultIcon()
    {
        // 아이콘이 없을 경우 보여줄 기본 아이콘 처리
        return null;
    }

    private void ClearUI()
    {
        icon.sprite = null;
        nameText.text = "";
        levelText.text = "";
        costText.text = "";
        progressText.text = "";
    }

    private void SetupButtonEvents()
    {
        if (actionButton == null || actionButtonText == null || deckManager == null)
        {
            Debug.LogWarning("[SlotUI] SetupButtonEvents 호출 시 필수 컴포넌트가 할당되지 않음");
            return;
        }

        actionButton.onClick.RemoveAllListeners();

        if (mode == SlotMode.Deck)
        {
            if (deckManager.isReplaceMode)
            {
                actionButtonText.text = "교체";
                actionButton.onClick.AddListener(() =>
                {
                    if (deckManager.replaceTargetCard == null)
                    {
                        Debug.LogWarning("[SlotUI] 교체 대상 카드가 없습니다.");
                        return;
                    }
                    Debug.Log($"ReplaceCard 호출 - 슬롯 인덱스: {slotIndex}");
                    deckManager.ReplaceCard(slotIndex, deckManager.replaceTargetCard);
                });
            }
            else
            {
                actionButtonText.text = "제거";
                actionButton.onClick.AddListener(() =>
                {
                    Debug.Log($"RemoveCard 호출 - 슬롯 인덱스: {slotIndex}");
                    deckManager.RemoveCard(slotIndex);
                });
            }
        }
        else if (mode == SlotMode.Collection)
        {
            actionButtonText.text = "사용";
            actionButton.onClick.AddListener(() =>
            {
                Debug.Log($"TryAddOrReplace 호출 - 카드: {cardData.cardName}");
                deckManager.TryAddOrReplace(cardData);
            });
        }
        else
        {
            actionButtonText.text = "";
            actionButton.interactable = false;
        }
    }

    public void ToggleInfoPanel()
    {
        if (infoPanel != null)
            infoPanel.SetActive(!infoPanel.activeSelf);
    }
}
