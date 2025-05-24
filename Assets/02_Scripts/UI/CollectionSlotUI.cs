using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotUI : MonoBehaviour
{
    public enum SlotMode { Deck, Collection }

    [Header("공용 UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text progressText;

    [Header("버튼")]
    [SerializeField] private Button actionButton;
    [SerializeField] private TMP_Text actionButtonText;
    [SerializeField] private Button infoButton;

    [Header("기타")]
    [SerializeField] private GameObject infoPanel; // 필요시 패널 열고 닫기

    private MonsterData_Mainmenu cardData;
    private SlotMode mode;
    private int slotIndex; // 덱 전용
    private bool isOwned;

    public void Init(MonsterData_Mainmenu data, SlotMode currentMode, bool owned = true, int index = -1)
    {
        cardData = data;
        mode = currentMode;
        isOwned = owned;
        slotIndex = index;

        if (cardData == null)
        {
            iconImage.sprite = null;
            iconImage.color = new Color(1, 1, 1, 0.05f);
            nameText.text = "";
            levelText.text = "";
            costText.text = "";
            progressText.text = "";
            actionButton.gameObject.SetActive(false);
            infoButton.gameObject.SetActive(false);
            return;
        }

        iconImage.sprite = cardData.icon;
        iconImage.color = isOwned ? Color.white : new Color(1, 1, 1, 0.3f);
        nameText.text = cardData.monsterName;
        levelText.text = $"레벨 {cardData.level}";
        costText.text = cardData.cost.ToString();

        SetupModeUI();
    }

    private void SetupModeUI()
    {
        actionButton.onClick.RemoveAllListeners();
        infoButton.onClick.RemoveAllListeners();

        if (mode == SlotMode.Deck)
        {
            progressText.gameObject.SetActive(true);
            int owned = DeckManagerRef().ownedCardDict.ContainsKey(cardData.id) ? DeckManagerRef().ownedCardDict[cardData.id] : 0;
            int required = DeckManagerRef().upgradeDB.GetRequiredCards(cardData.rarity, cardData.level);
            progressText.text = $"{owned}/{required}";

            actionButtonText.text = "제거";
            actionButton.colors = CreateColorBlock(Color.red);
            actionButton.onClick.AddListener(() => DeckManagerRef().RemoveCard(slotIndex));

            infoButton.onClick.AddListener(() => Debug.Log($"[Info: 덱] {cardData.monsterName} - {cardData.description}"));
        }
        else if (mode == SlotMode.Collection)
        {
            progressText.gameObject.SetActive(false);

            actionButtonText.text = "사용";
            actionButton.colors = CreateColorBlock(new Color(1f, 0.84f, 0f)); // 노란색
            actionButton.interactable = isOwned;
            actionButton.onClick.AddListener(() =>
            {
                if (!isOwned) return;
                DeckManagerRef().TryAddCard(cardData);
            });

            infoButton.onClick.AddListener(() => Debug.Log($"[Info: 컬렉션] {cardData.monsterName} - {cardData.description}"));
        }
    }

    private DeckManager_UI DeckManagerRef()
    {
        return FindObjectOfType<DeckManager_UI>();
    }

    private ColorBlock CreateColorBlock(Color baseColor)
    {
        return new ColorBlock
        {
            normalColor = baseColor,
            highlightedColor = baseColor * 1.2f,
            pressedColor = baseColor * 0.8f,
            selectedColor = baseColor,
            disabledColor = Color.gray,
            colorMultiplier = 1f,
            fadeDuration = 0.1f
        };
    }

    public void ToggleInfoPanel()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(!infoPanel.activeSelf);
        }
    }
}
