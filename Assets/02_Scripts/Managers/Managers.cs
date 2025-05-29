using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers _instance;
    private static bool _Init;
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

}
