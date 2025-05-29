using System.Collections;
using UnityEngine;
using Fusion;

[RequireComponent(typeof(NetworkObject))]
public class MonsterProjectile : NetworkBehaviour
{
    private Vector3 startPos;
    private Vector3 targetPos;
    private PlayerRef owner;
    private int damage;
    private float speed = 50f;

    private IDamageAble targetDamageAble; // ✅ 명중할 대상

    public void Init(Transform target, PlayerRef owner, int damage, float speed)
    {
        if (!target.TryGetComponent<IDamageAble>(out var dmg)) return;

        this.targetDamageAble = dmg; // ✅ 로컬에서 저장
        if (Object.HasStateAuthority)
        {
            Vector3 start = transform.position;
            Vector3 targetPosition = target.position;
            RPC_Launch(start, targetPosition, owner, damage, speed);
        }
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    private void RPC_Launch(Vector3 start, Vector3 target, PlayerRef owner, int dmg, float spd)
    {
        this.startPos = start;
        this.targetPos = target;
        this.owner = owner;
        this.damage = dmg;
        this.speed = spd;

        transform.position = startPos;
        StartCoroutine(MoveToTarget());
    }

    private IEnumerator MoveToTarget()
    {
        Vector3 prevPos = transform.position;

        float t = 0f;
        float dist = Vector3.Distance(startPos, targetPos);
        float duration = dist / speed;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            Vector3 pos = Vector3.Lerp(startPos, targetPos, t);
            transform.position = pos;

            Vector3 velocity = (transform.position - prevPos).normalized;
            if (velocity != Vector3.zero)
                transform.forward = velocity;

            prevPos = transform.position;
            yield return null;
        }

        // ✅ 명중한 대상이 null 아니고 아직 살아있으면
        if (targetDamageAble != null && targetDamageAble.PlayerRef != owner)
        {
            CombatSystem.Instance.AddCombatEvent(new CombatEvent
            {
                Receiver = targetDamageAble,
                Damage = damage,
                UseEffect = true,
                EffectName = "HitEffect",
                EffectPosition = targetDamageAble.GameObject.transform.position,
                NetworkObject = targetDamageAble.NetworkObject
            });
        }

        yield return new WaitForSeconds(0.1f);

        if (Object != null && Object.IsValid && Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
        }
    }
}