using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitSpawner : MonoBehaviour
{

    public GameObject unitPrefab;


    public void SpawnAt(Vector3 worldPosition)
    {
        if (unitPrefab == null)
        {
            Debug.LogWarning("EnemyPrefab이 할당되지 않았습니다.");
            return;
        }

        Instantiate(unitPrefab, worldPosition, Quaternion.identity);
    }

}
