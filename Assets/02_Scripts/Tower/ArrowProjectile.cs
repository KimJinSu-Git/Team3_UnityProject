using System.Collections;
using Fusion;
using UnityEngine;

public class ArrowProjectile : NetworkBehaviour
{
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Transform target;
    private PlayerRef caster;
    private int damage;

    private float speed = 20f;

    public void Init(Transform targetTransform, PlayerRef owner, int damage)
    {
        if (Object.HasStateAuthority)
        {
            Vector3 start = transform.position;
            Vector3 end = targetTransform.position;
            var targetObj = targetTransform.GetComponent<NetworkObject>();
            if (targetObj != null)
            {
                RPC_Launch(start, end, targetObj, owner, damage);
            }
        }
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    private void RPC_Launch(Vector3 start, Vector3 targetPos, NetworkObject targetObj, PlayerRef owner, int dmg)
    {
        this.startPosition = start;
        this.targetPosition = targetPos;
        this.target = targetObj.transform;
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

        if (target != null && target.TryGetComponent<IDamageAble>(out var dmg) && dmg.PlayerRef != caster)
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

        yield return new WaitForSeconds(0.05f);

        if (Object != null && Object.IsValid && Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
        }
    }
}