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

    [Header("Effect Info")]
    public float range;       // 범위 반경
    public float damage;      // 데미지
    public float duration;    // 범위 지속 시간
    public float delay;       // 이펙트 발동 전 딜레이 (ex: 화살이 하늘에서 떨어지는 시간 등)

    [Header("SpawnInfo")]
    public int cost;
    public float cooldown;      
    
    [Header("Image")]
    public Sprite icon;
    public Sprite castingCircle; // 사전 표시용 범위 이미지
    public GameObject spellPrefab;
}