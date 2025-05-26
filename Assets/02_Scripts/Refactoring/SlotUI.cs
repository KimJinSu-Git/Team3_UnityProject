// ✅ SlotUI - 덱/콜렉션 카드 슬롯 UI
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

        icon.sprite = monster.icon;
        nameText.text = monster.monsterName;
        levelText.text = $"Lv.{card.level}";
        costText.text = monster.cost.ToString();

        int owned = card.ownedCount;
        int required = deckManager.upgradeDB.GetRequiredCards(monster.rarity, card.level);
        progressText.text = $"{owned}/{required}";

        SetupModeUI();
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
            if (cardData.monsterData != null)
                Debug.Log($"[Info] {cardData.monsterData.monsterName} - {cardData.monsterData.description}");
        });
    }
}