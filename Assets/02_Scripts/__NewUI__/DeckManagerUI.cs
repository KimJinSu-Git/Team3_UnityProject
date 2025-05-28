using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;

public class DeckManagerUI : MonoBehaviour
{
    PlayerCombatDeckSlot[] playerCombatDeckSlots = new PlayerCombatDeckSlot[6];
    Transform slotParent;
    void Awake()
    {
        slotParent = transform.GetChild(0);
        for (int i = 0; i < playerCombatDeckSlots.Length; i++)
        {
            playerCombatDeckSlots[i] = slotParent.GetChild(i).GetComponent<PlayerCombatDeckSlot>();
        }
    }

    private void Start()
    {
        DeckManager.Instance.OnCurrentDeckReady += SetPlayerCombatDeckSlots;
    }
    void SetPlayerCombatDeckSlots()
    {
        for (int i = 0; i < playerCombatDeckSlots.Length; i++)
        {
            playerCombatDeckSlots[i].baseData = DeckManager.Instance.currentPlayerDeck[i];
        }
    }
}
