using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager
{
    private List<GameObject> effectPrefabs;
    public void Init()
    {
        //effectPrefabs.Add(Resources.Load<GameObject>("Arrow"));

        for (int i = 0; i < effectPrefabs.Count; i++)
        {
            // Pool 세팅
            Managers.PoolManager.ObjInit(effectPrefabs[i],3);
        }
    }
}
