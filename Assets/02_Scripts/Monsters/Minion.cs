using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion : BaseMonster
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
