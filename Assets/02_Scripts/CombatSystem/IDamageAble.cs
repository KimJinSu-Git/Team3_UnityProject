using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageAble
{
    public GameObject GameObject { get; }
    public Collider Collider { get; }
    public void TakeDamage(int combatEvent, bool OnDamage);
}
