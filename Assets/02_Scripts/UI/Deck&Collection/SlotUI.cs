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
    private MonsterData_Mainmenu cardData;
    private int slotIndex;
    private SlotMode mode;
    
    
    
    private void Awake()
    {
        // 보통 Setup() 또는 Start()에서 초기화
        infoButton.onClick.AddListener(OnClickSlot);
        infoPanel.SetActive(false); // 시작 시 꺼두기
    }
    
    public void OnClickSlot()
    {
        bool isActive = infoPanel.activeSelf;
        infoPanel.SetActive(!isActive); // 토글
    }

    public void Setup(MonsterData_Mainmenu card, int index, DeckManager_UI manager, SlotMode slotMode)
    {
        
        
        cardData = card;
        slotIndex = index;
        deckManager = manager;
        mode = slotMode;

        icon.sprite = card.icon;
        nameText.text = card.monsterName;
        levelText.text = $"Lv.{card.level}";
        costText.text = card.cost.ToString();
        
        int owned = manager.ownedCardDict.ContainsKey(card.id) ? manager.ownedCardDict[card.id] : 0;
        int required = manager.upgradeDB.GetRequiredCards(card.rarity, card.level);
        
        if (manager == null)
        {
            Debug.LogError("[SlotUI] ❌ manager is null in Setup!");
            return;
        }
        if (manager.ownedCardDict == null) Debug.LogError("❌ ownedCardDict is null!");
        if (manager.upgradeDB == null) Debug.LogError("❌ upgradeDB is null!");
        
        progressText.text = $"{owned}/{required}";

        SetupModeUI();
    }

    private void SetupModeUI()
    {
        actionButton.onClick.RemoveAllListeners();

        if (mode == SlotMode.Deck)
        {
            actionButtonText.text = "제거";
            actionButton.onClick.AddListener(() => deckManager.RemoveCard(slotIndex));
        }
        else if (mode == SlotMode.Collection)
        {
            actionButtonText.text = "사용";
            actionButton.onClick.AddListener(() => deckManager.TryAddCard(cardData));
        }

        infoButton.onClick.AddListener(() =>
            Debug.Log($"[Info] {cardData.monsterName} - {cardData.description}")
        );
    }
}