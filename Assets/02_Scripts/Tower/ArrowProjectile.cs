using System;
using System.Collections;
using Fusion;
using UnityEngine;

public class ArrowProjectile : NetworkBehaviour
{
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private PlayerRef caster;
    private int damage;

    private float speed = 20f;

    public void Init(Vector3 targetPos, PlayerRef owner, int damage)
    {
        if (Object.HasStateAuthority)
        {
            Vector3 start = transform.position;
            RPC_Launch(start, targetPos, owner, damage);
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

        Collider[] hits = Physics.OverlapSphere(transform.position, 0.5f, LayerMask.GetMask("Monster", "Tower"));
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageAble>(out var receiver) && receiver.PlayerRef != caster)
            {
                CombatEvent combatEvent = new CombatEvent
                {
                    Receiver = receiver,
                    Damage = damage,
                    UseEffect = true,
                    EffectName = "ArrowHit",
                    EffectPosition = receiver.GameObject.transform.position,
                    NetworkObject = receiver.NetworkObject
                };

                CombatSystem.Instance.AddCombatEvent(combatEvent);
            }
        }

        yield return new WaitForSeconds(0.05f);

        if (Object != null && Object.IsValid && Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
        }
    }
}
