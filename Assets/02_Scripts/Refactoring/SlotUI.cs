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

    private DeckManager_UI deckManager;
    private PlayerCardData cardData;
    private int slotIndex;
    private SlotMode mode;

    private void Awake()
    {
        infoButton.onClick.AddListener(OnClickSlot);
        infoPanel.SetActive(false);
    }

    public void OnClickSlot()
    {
        bool isActive = infoPanel.activeSelf;
        infoPanel.SetActive(!isActive);
    }

    public void Setup(PlayerCardData card, int index, DeckManager_UI manager, SlotMode slotMode)
    {
        cardData = card;
        slotIndex = index;
        deckManager = manager;
        mode = slotMode;

        var monster = card.monsterData;

        if (monster == null)
        {
            Debug.LogError("[SlotUI] monsterData 연결되지 않음");
            return;
        }

        // ✅ 스킬 데이터 자동 세팅 (cardData.skillName 기준)
        if (card.skillData == null && !string.IsNullOrEmpty(card.id))
        {
            card.skillData = SkillManager.Instance.GetSkillData(card.id);
            if (card.skillData == null)
                Debug.LogWarning($"[SlotUI] skillData 찾을 수 없음: {card.id}");
        }

        icon.sprite = monster.icon;
        nameText.text = monster.name;
        levelText.text = $"Lv.{card.level}";
        costText.text = monster.cost.ToString();

        UpdateProgressUI();

        // 카드 변경 이벤트 등록 (중복 방지 주의)
        deckManager.inventory.onCardChanged.AddListener((changedId) =>
        {
            if (changedId == card.id)
            {
                UpdateProgressUI();
            }
        });

        SetupModeUI();
    }

    private void UpdateProgressUI()
    {
        int owned = deckManager.inventory.GetCardCount(cardData.id);
        int required = deckManager.upgradeDB.GetRequiredCards(cardData.monsterData.rarity, cardData.level);

        if (required <= 0 || required >= 1000000)
            progressText.text = "-";
        else
            progressText.text = $"{owned}/{required}";
    }

    private void SetupModeUI()
    {
        actionButton.onClick.RemoveAllListeners();

        if (mode == SlotMode.Deck)
        {
            if (deckManager.isReplaceMode)
            {
                actionButtonText.text = "교체";
                actionButton.onClick.AddListener(() => deckManager.ReplaceCard(slotIndex));
            }
            else
            {
                actionButtonText.text = "제거";
                actionButton.onClick.AddListener(() => deckManager.RemoveCard(slotIndex));
            }
        }
        else if (mode == SlotMode.Collection)
        {
            actionButtonText.text = "사용";
            actionButton.onClick.AddListener(() => deckManager.TryAddOrReplace(cardData));
        }

        infoButton.onClick.AddListener(() =>
        {
            string log = $"[Info] {cardData.monsterData.name} - {cardData.monsterData.description}";
            if (cardData.skillData != null)
            {
                log += $"\n[Skill] {cardData.skillData.name} - {cardData.skillData.description}";
            }
            Debug.Log(log);
        });
    }
}
