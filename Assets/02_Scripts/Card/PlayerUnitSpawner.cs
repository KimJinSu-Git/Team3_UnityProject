using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitSpawner : MonoBehaviour
{
    /// <summary>
    /// 지정된 UnitType의 unitPrefab을 worldPosition에 스폰.
    /// </summary>
    public void SpawnAt(MonsterData type, Vector3 worldPosition)
    {
        Instantiate(type.prefab, worldPosition, Quaternion.identity, transform);
    }

}
