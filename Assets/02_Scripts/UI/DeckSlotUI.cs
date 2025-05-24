using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckSlotUI : MonoBehaviour
{
    [Header("UI 구성 요소")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text costText;

    [Header("상세 패널")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private Button infoButton;
    [SerializeField] private Button removeButton;

    private MonsterData_Mainmenu data;
    private DeckManager_UI manager;
    private int slotIndex;

    public void Init(MonsterData_Mainmenu monster, int index, DeckManager_UI deckManager)
    {
        data = monster;
        manager = deckManager;
        slotIndex = index;

        if (data != null)
        {
            iconImage.sprite = data.icon;
            iconImage.color = Color.white;

            nameText.text = data.monsterName;
            levelText.text = $"레벨 {data.level}";

            int owned = manager.ownedCardDict.ContainsKey(data.id) ? manager.ownedCardDict[data.id] : 0;
            int required = manager.upgradeDB.GetRequiredCards(data.rarity, data.level);
            progressText.text = $"{owned}/{required}";

            costText.text = data.cost.ToString();
        }
        else
        {
            // 빈칸 처리
            iconImage.sprite = null;
            iconImage.color = new Color(1, 1, 1, 0.05f);
            nameText.text = "";
            levelText.text = "";
            progressText.text = "";
            costText.text = "";
        }

        // 패널 초기화
        infoPanel.SetActive(false);

        // 버튼 리스너 중복 방지
        infoButton.onClick.RemoveAllListeners();
        removeButton.onClick.RemoveAllListeners();

        infoButton.onClick.AddListener(ShowInfo);
        removeButton.onClick.AddListener(RemoveFromDeck);
    }

    public void OnClickSlot()
    {
        // 슬롯 누르면 상세 패널 토글
        if (data != null)
        {
            infoPanel.SetActive(!infoPanel.activeSelf);
        }
    }

    private void ShowInfo()
    {
        Debug.Log($"[Info] {data.monsterName} - 설명: {data.description}");
        // 추후 팝업 시스템과 연동 가능
    }

    private void RemoveFromDeck()
    {
        manager.RemoveCard(slotIndex);
    }
}
