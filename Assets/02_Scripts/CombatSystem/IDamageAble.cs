using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public interface IDamageAble
{
    public GameObject GameObject { get; }
    public Collider Collider { get; }
    public PlayerRef PlayerRef { get; }
    public NetworkObject NetworkObject { get; }
    public void TakeDamage(int damage);
}
