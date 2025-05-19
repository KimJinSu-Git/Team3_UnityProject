using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenCastleManager : MonoBehaviour
{
    public static event Action<int> OnBrokenCastle;

    public static void OnTriggerCastleBroken(int brokenEvent)
    {
        OnBrokenCastle?.Invoke(brokenEvent);
    }
}
