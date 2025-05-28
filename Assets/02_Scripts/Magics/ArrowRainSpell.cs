using System.Collections;
using Fusion;
using UnityEngine;

/// <summary>
/// 화살비 마법: 일정 딜레이 후 범위 내 적에게 데미지
/// </summary>
public class ArrowRainSpell : NetworkBehaviour
{
    public GameObject arrowProjectilePrefab;
    public int arrowCount = 10;
    
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

        transform.position = targetPos;

        if (Object.HasStateAuthority)
        {
            RPC_ActivateArrowRain();
        }
    }
    
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    private void RPC_ActivateArrowRain()
    {
        StartCoroutine(Activate());
    }
    
    private IEnumerator Activate()
    {
        yield return new WaitForSeconds(skillData.delay);

        for (int i = 0; i < arrowCount; i++)
        {
            Vector2 offset2D = UnityEngine.Random.insideUnitCircle * skillData.range;
            Vector3 spawnPos = kingTower.position + Vector3.up * 2f;
            Vector3 target = targetPosition + new Vector3(offset2D.x, 0, offset2D.y);
            
            NetworkObject arrowObj = Runner.Spawn(arrowProjectilePrefab.GetComponent<NetworkObject>(), spawnPos, Quaternion.identity, caster);
            ArrowRainProjectile arrow = arrowObj.GetComponent<ArrowRainProjectile>();
            arrow.Init(target, caster, Mathf.RoundToInt(skillData.damage));
            // GameObject arrow = Instantiate(arrowProjectilePrefab, spawnPos, Quaternion.identity);
            // arrow.GetComponent<ArrowRainProjectile>().Init(target, caster, Mathf.RoundToInt(skillData.damage));
        }

        yield return new WaitForSeconds(2f);

        if (Object != null && Object.IsValid && Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
        }

    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (skillData == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, skillData.range);
    }
#endif
}