using System.Collections;
using Fusion;
using UnityEngine;

/// <summary>
/// FireballSpell은 아군 킹 타워에서 화염구 발사체를 날림
/// </summary>
public class FireballSpell : NetworkBehaviour
{
    public GameObject fireballProjectilePrefab;
    
    private SkillData skillData;
    private PlayerRef caster;
    private Vector3 targetPosition;
    private Transform kingTower;

    public void Init(SkillData data, Vector3 targetPos, PlayerRef owner, Transform kingTowerTransform)
    {
        skillData = data;
        caster = owner;
        targetPosition = targetPos;
        kingTower = kingTowerTransform;

        if (Object.HasStateAuthority)
        {
            RPC_ActivateFireball();
        }
    }
    
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    private void RPC_ActivateFireball() 
    {
        StartCoroutine(Fire());
    }

    private IEnumerator Fire()
    {
        Vector3 spawnPos = kingTower.position + Vector3.up * 2f;

        // NetworkObject로 가져오기 (필수!)
        NetworkObject fireballNetObj = Runner.Spawn(fireballProjectilePrefab.GetComponent<NetworkObject>(), spawnPos, Quaternion.identity, caster);

        FireballProjectile fireball = fireballNetObj.GetComponent<FireballProjectile>();
        fireball.Init(targetPosition, caster, Mathf.RoundToInt(skillData.damage));

        yield return new WaitForSeconds(2f);

        if (Object != null && Object.IsValid && Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
        }
    }
    
    
}