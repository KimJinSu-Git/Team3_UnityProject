using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageAble
{
    public GameObject GameObject { get; }
    public Collider Collider { get; }
    
    public OwnerPlayerType PlayerType { get; }
    public void TakeDamage(int combatEvent, bool OnDamage);
}
