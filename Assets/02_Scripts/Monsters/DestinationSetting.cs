using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationSetting : MonoBehaviour
{
    public Transform[] targetPosition;
    public LayerType LayerDes;
}

public enum LayerType
{
    Default,
    TransparentFX,
    IgnoreRaycast,
    Water,
    UI,
    EnemyUnit,
    PlayerUnit,
}
