using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "ScriptableObjects/CardData")]
public class CardData : ScriptableObject
{
    [Header("Identification")]
    public string unitName;

    [Header("Prefabs")]
    public GameObject unitPrefab;      // 실제 스폰할 프리팹
    public GameObject previewPrefab;   // 드래그 미리보기용 프리팹
}