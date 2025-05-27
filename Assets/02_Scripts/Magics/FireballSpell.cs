using System.Collections;
using Fusion;
using UnityEngine;

/// <summary>
/// FireballSpell은 아군 킹 타워에서 화염구 발사체를 날림
/// </summary>
public class FireballSpell : NetworkBehaviour
{
    public GameObject fireballProjectilePrefab;
    public Transform kingTowerTransform;
    
    private SkillData skillData;
    private PlayerRef caster;
    private Vector3 targetPosition;

    public void Init(SkillData data, Vector3 targetPos, PlayerRef owner, Transform kingTower)
    {
        skillData = data;
        caster = owner;
        targetPosition = targetPos;
        kingTowerTransform = kingTower;

        // 시작 위치 = 아군 킹 타워 위치 + 약간 위
        Vector3 startPos = kingTowerTransform.position + Vector3.up * 2f;

        // 발사체 생성 및 초기화
        GameObject fireball = Instantiate(fireballProjectilePrefab, startPos, Quaternion.identity);

        var projectile = fireball.GetComponent<FireballProjectile>();
        projectile.Init(targetPosition, caster, Mathf.RoundToInt(skillData.damage));

        Destroy(gameObject, 2);
    }
    
    
}