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

        transform.position = targetPos;
        StartCoroutine(Activate());
    }

    private IEnumerator Activate()
    {
        yield return new WaitForSeconds(skillData.delay);

        for (int i = 0; i < arrowCount; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * skillData.range;
            Vector3 offset = new Vector3(randomCircle.x, 0f, randomCircle.y);
            Vector3 targetPos = targetPosition + offset;

            // 아군 킹타워 위치에서 날아옴
            Vector3 spawnPos = kingTowerTransform.position + Vector3.up * 2f;

            GameObject arrow = Instantiate(arrowProjectilePrefab, spawnPos, Quaternion.identity);
            arrow.GetComponent<ArrowRainProjectile>().Init(targetPos, caster, Mathf.RoundToInt(skillData.damage));
        }

        Destroy(gameObject, 2f);
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