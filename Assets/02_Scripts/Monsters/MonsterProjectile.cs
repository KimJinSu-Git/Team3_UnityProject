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
    private float speed = 10f;

    public void Init(Vector3 targetPos, PlayerRef owner, int damage, float speed)
    {
        if (Object.HasStateAuthority)
        {
            Vector3 start = transform.position + Vector3.up * 2f;
            RPC_Launch(start, targetPos, owner, damage, speed);
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

        transform.position = start;
        StartCoroutine(MoveToTarget());
    }

    private IEnumerator MoveToTarget()
    {
        Vector3 dir = (targetPos - startPos).normalized;
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

        // 타격 판정
        Collider[] hits = Physics.OverlapSphere(transform.position, 0.5f, LayerMask.GetMask("Monster", "Tower"));
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageAble>(out var dmg) && dmg.PlayerRef != owner)
            {
                CombatSystem.Instance.AddCombatEvent(new CombatEvent
                {
                    Receiver = dmg,
                    Damage = damage,
                    UseEffect = true,
                    EffectName = "HitEffect",
                    EffectPosition = dmg.GameObject.transform.position,
                    NetworkObject = dmg.NetworkObject
                });
            }
        }

        yield return new WaitForSeconds(0.1f);
        if (Object != null && Object.IsValid && Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
        }
    }
}