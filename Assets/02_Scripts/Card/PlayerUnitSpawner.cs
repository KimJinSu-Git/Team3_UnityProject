using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitSpawner : MonoBehaviour
{
    /// <summary>
    /// 지정된 UnitType의 unitPrefab을 worldPosition에 스폰.
    /// </summary>
    public void SpawnAt(CardData type, Vector3 worldPosition)
    {
        if (type == null || type.unitPrefab == null)
        {
            Debug.LogWarning("UnitType 또는 prefab이 할당되지 않았습니다.");
            return;
        }
        Instantiate(type.unitPrefab, worldPosition, Quaternion.identity, transform);
    }

}
