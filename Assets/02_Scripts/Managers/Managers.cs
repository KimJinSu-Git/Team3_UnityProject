using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers _instance;
    private static bool _Init;

    PoolManager poolManager = new PoolManager();
    EffectManager effectManager = new EffectManager();
    
    public static PoolManager PoolManager { get { return _instance.poolManager; } }
    public static EffectManager EffectManager { get { return _instance.effectManager; } }
    public static Managers Instance
    {
        get
        {
            if (_Init == false)
            {
                _Init = true;
                GameObject go = GameObject.Find("@Managers");
                if (go != null)
                {
                    go = new GameObject("@Managers");
                    go.AddComponent<Managers>();
                }
                DontDestroyOnLoad(go);
                _instance = go.GetComponent<Managers>();
            }
            return _instance;
        }
    }

    public void Start()
    {
        EffectManager.Init();
    }
}
