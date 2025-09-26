using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public interface IDamageAble
{
    public GameObject GameObject { get; }
    public Collider Collider { get; }
    public NetworkObject NetworkObject { get; }
    public PlayerRef PlayerRef { get; }
    public void TakeDamage(int damage);
}
