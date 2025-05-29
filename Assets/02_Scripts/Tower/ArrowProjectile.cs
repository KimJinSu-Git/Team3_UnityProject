using System.Collections;
using UnityEngine;
using Fusion;

[RequireComponent(typeof(NetworkObject))]
public class ArrowProjectile : NetworkBehaviour
{
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private PlayerRef caster;
    private int damage;
    private float speed = 20f;
    private Transform targetTransform;
    private PlayerRef targetOwner;

    public void Init(Transform target, PlayerRef owner, int damage)
    {
        if (Object.HasStateAuthority)
        {
            Vector3 start = transform.position;
            RPC_Launch(start, target.position, owner, damage);

            targetTransform = target;

            if (target.TryGetComponent<IDamageAble>(out var dmg))
                targetOwner = dmg.PlayerRef;
        }
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    private void RPC_Launch(Vector3 start, Vector3 target, PlayerRef owner, int dmg)
    {
        this.startPosition = start;
        this.targetPosition = target;
        this.caster = owner;
        this.damage = dmg;

        transform.position = startPosition;
        StartCoroutine(MoveToTarget());
    }

    private IEnumerator MoveToTarget()
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.2f)
        {
            Vector3 dir = (targetPosition - transform.position).normalized;
            transform.position += dir * (speed * Time.deltaTime);

            if (dir != Vector3.zero)
                transform.forward = dir;

            yield return null;
        }

        if (targetTransform != null && targetTransform.TryGetComponent<IDamageAble>(out var dmg))
        {
            if (dmg.PlayerRef != caster && dmg.PlayerRef == targetOwner)
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

        yield return new WaitForSeconds(0.05f);

        if (Object != null && Object.IsValid && Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
        }
    }
}