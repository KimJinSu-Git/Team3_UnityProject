using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class ArrowRainProjectile : MonoBehaviour
{
    private Vector3 targetPos;
    private PlayerRef caster;
    private int damage;
    private float speed = 20f;

    public void Init(Vector3 targetPos, PlayerRef caster, int damage)
    {
        this.targetPos = targetPos;
        this.caster = caster;
        this.damage = damage;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.2f)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, 0.5f, LayerMask.GetMask("Monster", "Tower"));
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<IDamageAble>(out var dmg) && dmg.PlayerRef != caster)
                {
                    CombatSystem.Instance.AddCombatEvent(new CombatEvent
                    {
                        Sender = null,
                        Receiver = dmg,
                        Damage = damage,
                        UseEffect = true,
                        EffectName = "ArrowHit",
                        EffectPosition = dmg.GameObject.transform.position,
                        NetworkObject = dmg.NetworkObject
                    });
                }
            }

            Destroy(gameObject);
        }
    }
}
