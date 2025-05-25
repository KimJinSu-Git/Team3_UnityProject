using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class CombatEvent : MonoBehaviour
{
    public IDamageAble Sender { get; set; }
    public IDamageAble Receiver { get; set; }
    public int Damage { get; set; }
    public Collider Collider { get; set; }
    public NetworkObject NetworkObject { get; set; }
    public bool UseEffect { get; set; }
    public Vector3 EffectPosition { get; set; }
    public string EffectName { get; set; }
}
