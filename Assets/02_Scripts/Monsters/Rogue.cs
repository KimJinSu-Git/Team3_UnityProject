using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rogue : BaseMonster
{
    void Start()
    {
        base.Start();

    }

    void Update()
    {
        base.FixedUpdateNetwork();
    }
}
