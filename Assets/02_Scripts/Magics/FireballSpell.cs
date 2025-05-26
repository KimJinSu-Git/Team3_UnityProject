using System.Collections;
using Fusion;
using UnityEngine;

/// <summary>
/// 불덩이 마법: 일정 딜레이 후 범위 폭발로 데미지
/// </summary>
public class FireballSpell : NetworkBehaviour
{
    private SkillData skillData;
    private PlayerRef caster;
    private Vector3 targetPosition;

    public void Init(SkillData data, Vector3 targetPos, PlayerRef owner)
    {
        skillData = data;
        caster = owner;
        targetPosition = targetPos;

        transform.position = targetPosition;

        // 필요 시 초기 이펙트 재생
        StartCoroutine(Activate());
    }

    private IEnumerator Activate()
    {
        // 불덩이가 날아오는 시간
        yield return new WaitForSeconds(skillData.delay);

        // 폭발 이펙트 재생 위치 등 처리 가능

        Collider[] hits = Physics.OverlapSphere(transform.position, skillData.range, LayerMask.GetMask("Monster", "Tower"));
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageAble>(out var dmg) && dmg.PlayerRef != caster)
            {
                CombatSystem.Instance.AddCombatEvent(new CombatEvent
                {
                    Sender = null,
                    Receiver = dmg,
                    Damage = Mathf.RoundToInt(skillData.damage),
                    UseEffect = true,
                    EffectName = skillData.skillName, 
                    EffectPosition = dmg.GameObject.transform.position,
                    NetworkObject = dmg.NetworkObject
                });
            }
        }

        // 이펙트 종료 후 파괴
        Destroy(gameObject, 1f);
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