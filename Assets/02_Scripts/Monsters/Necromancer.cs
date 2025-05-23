using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Necromancer : BaseMonster
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
