using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCombatDeckSlot : MonoBehaviour
{
    public BaseData baseData;
    Image image;
    void Awake()
    {
        TryGetComponent(out image);
    }
    void Update()
    {
        if (baseData != null)
        {
            image.sprite = baseData.icon;
        }
    }
}
