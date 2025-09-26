using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;

public class DeckManagerUI : MonoBehaviour
{
    [Header("슬롯 관련")]
    public GameObject slotPrefab;          // 새 SlotUI 프리팹
    public List<Transform> slotParents;    // 슬롯이 생성될 부모 오브젝트

    private List<GameObject> currentSlots = new List<GameObject>();
    
    public bool isReplaceMode = false; // 교체 모드 활성화 여부
    
    public BaseData replaceTargetCard;

    void Start()
    {
        DeckManager.Instance.OnCurrentDeckReady += CreateDeckSlots;
    }

    void CreateDeckSlots()
    {
        ClearSlots();

        var deck = DeckManager.Instance.currentPlayerDeck;
        var deckManagerUI = GetComponent<DeckManagerUI>(); // SlotUI에서 필요

        for (int i = 0; i < deck.Count; i++)
        {
            var parentTransform = slotParents[i];  // 부모 리스트에서 i번째 Transform 가져오기
            var go = Instantiate(slotPrefab, parentTransform);
            var slotUI = go.GetComponent<SlotUI>();

            if (slotUI != null)
            {
                slotUI.Setup(deck[i], i, deckManagerUI, SlotMode.Deck);
                currentSlots.Add(go);
            }
        }
    }

    void ClearSlots()
    {
        foreach (var go in currentSlots)
        {
            Destroy(go);
        }
        currentSlots.Clear();
    }
    
    public void ReplaceCard(int slotIndex, BaseData newCard)
    {
        if (!isReplaceMode)
        {
            Debug.LogWarning("교체 모드가 아닙니다.");
            return;
        }

        if (slotIndex < 0 || slotIndex >= DeckManager.Instance.currentPlayerDeck.Count)
        {
            Debug.LogError("잘못된 슬롯 인덱스");
            return;
        }

        DeckManager.Instance.currentPlayerDeck[slotIndex] = newCard;

        isReplaceMode = false;
        replaceTargetCard = null;

        CreateDeckSlots();
        DeckManager.Instance.collectionPanel.Refresh();

        DeckManager.Instance.DeckSave(); // 🔹 저장 추가
    }



    public void RemoveCard(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= DeckManager.Instance.currentPlayerDeck.Count)
            return;

        DeckManager.Instance.currentPlayerDeck.RemoveAt(slotIndex);

        isReplaceMode = false;
        replaceTargetCard = null;

        CreateDeckSlots();
        DeckManager.Instance.collectionPanel.Refresh();

        DeckManager.Instance.DeckSave(); // 🔹 저장 추가
    }

    public void TryAddOrReplace(BaseData cardData)
    {
        if (DeckManager.Instance.currentPlayerDeck.Count < slotParents.Count)
        {
            DeckManager.Instance.currentPlayerDeck.Add(cardData);
            CreateDeckSlots();
            DeckManager.Instance.collectionPanel.Refresh();
            isReplaceMode = false;
            replaceTargetCard = null;

            DeckManager.Instance.DeckSave(); // 🔹 저장 추가
        }
        else
        {
            isReplaceMode = true;
            replaceTargetCard = cardData;
            Debug.Log($"교체 모드 활성화: {cardData.cardName}");
            CreateDeckSlots();
            DeckManager.Instance.collectionPanel.Refresh();
        }
    }
}