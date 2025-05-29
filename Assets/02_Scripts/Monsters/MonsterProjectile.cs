using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkTransform))]
public class MonsterProjectile : NetworkBehaviour
{
    private Transform target;
    private PlayerRef owner;
    private int damage;
    private float speed;

    public void Init(Transform target, PlayerRef owner, int damage, float speed)
    {
        this.target = target;
        this.owner = owner;
        this.damage = damage;
        this.speed = speed;

        if (Object.HasStateAuthority)
            StartCoroutine(DestroySelf(3f));
    }

    private void Update()
    {
        if (target == null)
        {
            if (Object.HasStateAuthority)
                Runner.Despawn(Object);
            return;
        }

        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * (speed * Time.deltaTime);

        if (dir != Vector3.zero)
            transform.forward = dir;

        if (Vector3.Distance(transform.position, target.position) < 0.5f)
        {
            if (target.TryGetComponent<IDamageAble>(out var dmg) && dmg.PlayerRef != owner)
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

            if (Object.HasStateAuthority)
                Runner.Despawn(Object);
        }
    }

    private System.Collections.IEnumerator DestroySelf(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (Object != null && Object.IsValid)
            Runner.Despawn(Object);
    }
}