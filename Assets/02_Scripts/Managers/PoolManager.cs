using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager
{
    public Dictionary<string , Pool> pools= new Dictionary<string, Pool>();
    
    public void ObjInit(GameObject go, int count, Transform transform)
    {
        Pool pool = new Pool();
        pool.poolCreate(go, transform);
        for (int i = 0; i < count; i++)
        {
            GameObject enemy = Object.Instantiate(pool.Original, pool.ObjParent.transform);          
            enemy.name = pool.Original.name;
            enemy.SetActive(false);

            pool.poolQueue.Enqueue(enemy);
        }
        pools.Add(pool.Original.name, pool);
        
    }
    public void ObjInit(GameObject go, int count)
    {
        Pool pool = new Pool();
        pool.poolCreate(go);
        for (int i = 0; i < count; i++)
        {
            GameObject enemy = Object.Instantiate(pool.Original, pool.ObjParent.transform);
            enemy.name = pool.Original.name;
            enemy.SetActive(false);

            pool.poolQueue.Enqueue(enemy);
        }
        pools.Add(pool.Original.name, pool);
    }
    public GameObject ObjPop(string key,Vector3 transform)
    {
        GameObject go = null;
        if (pools[key].poolQueue.Count != 0)
        {
            go = pools[key].poolQueue.Dequeue();
            go.transform.position = transform;
            go.SetActive(true);
        }
        else
        {
            go = Object.Instantiate(pools[key].Original, transform, Quaternion.identity, pools[key].ObjParent);
            go.name = pools[key].Original.name;
        }
        pools[key].count++;

        return go;
    }
    public void ObjPush(GameObject go, string key)
    {
        go.SetActive(false);
        pools[key].poolQueue.Enqueue(go);
        pools[key].count--;
    }
    public int enemyCount(string key)
    {
        return pools[key].count;
    }
    public class Pool
    {
        public GameObject Original;
        public Transform ObjParent;
        public Queue<GameObject> poolQueue = new Queue<GameObject>();
        public int count = 0;

        public void poolCreate(GameObject go, Transform transform = null)
        {
            Original = go;
            GameObject objParent = new GameObject { name = $"{Original.name}_pool" };
            ObjParent = objParent.transform;
            ObjParent.transform.parent = transform;
        }
    }
}
