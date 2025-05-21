using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "ScriptableObjects/CardData")]
public class CardData : ScriptableObject
{
    [Header("Identification")]
    public string unitName; // Monster.monsterName과 같다.

    [Header("Prefabs")]
    public GameObject unitPrefab;      // Monster.prefab 실제 스폰할 프리팹
    public GameObject previewPrefab;   // Monster.previewPrefab 드래그 미리보기용 프리팹
}