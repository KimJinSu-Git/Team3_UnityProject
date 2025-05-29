using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum SlotMode { Deck, Collection }

public class SlotUI : MonoBehaviour
{
    [Header("공용")]
    public Image icon;
    public TMP_Text nameText, levelText, costText, progressText;

    [Header("버튼")]
    public Button actionButton;
    public TMP_Text actionButtonText;
    public Button infoButton;
    public GameObject infoPanel;

    private DeckManagerUI deckManager;
    private BaseData cardData;
    private int slotIndex;
    private SlotMode mode;

    private void Awake()
    {
        infoButton.onClick.AddListener(ToggleInfoPanel);
        infoPanel.SetActive(false);
    }

    public void Setup(BaseData card, int index, DeckManagerUI manager, SlotMode slotMode)
    {
        cardData = card;
        slotIndex = index;
        deckManager = manager;
        mode = slotMode;

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
            Debug.LogError("[SlotUI] 알 수 없는 카드 타입");
        }

        SetupButtonEvents();
    }

    private void SetupMonsterData(MonsterData monster)
    {
        icon.sprite = monster.icon;
        nameText.text = monster.cardName;
        levelText.text = $"Lv.{monster.level}";
        costText.text = monster.cost.ToString();
    }

    private void SetupSkillData(SkillData skill)
    {
        icon.sprite = skill.icon;
        nameText.text = skill.cardName;
        levelText.text = ""; // 스킬은 레벨 없을 수 있음
        costText.text = skill.cost.ToString();
    }

    private void SetupButtonEvents()
    {
        actionButton.onClick.RemoveAllListeners();

        if (mode == SlotMode.Deck)
        {
            if (deckManager.isReplaceMode)
            {
                actionButtonText.text = "교체";
                actionButton.onClick.AddListener(() =>
                {
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
    }


    public void ToggleInfoPanel()
    {
        infoPanel.SetActive(!infoPanel.activeSelf);
    }
}

