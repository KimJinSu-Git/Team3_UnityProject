using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "ScriptableObjects/SkillData")]
public class SkillData : ScriptableObject
{
    [Header("BaseInfo")]
    public string id;
    public string skillName; // 프리팹이름이랑 같게, 이걸 키값으로 Effect사용
    public string description;
    public bool isSelected;

    [Header("Range")]
    public float range; // 범위    
    public float duration; // 지속시간

    [Header("SpawnInfo")]
    public int cost;
    
    [Header("Image")]
    public Sprite icon;
    public GameObject castingCircle; // 범위 이미지
    public GameObject fireballShape;
}