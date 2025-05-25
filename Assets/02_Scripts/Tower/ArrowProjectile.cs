using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    private Transform target;
    private int damage;
    private IDamageAble sender;

    public void Init(Transform target, int damage, IDamageAble sender)
    {
        this.target = target;
        this.damage = damage;
        this.sender = sender;
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, 20f * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.2f)
        {
            if (target.TryGetComponent<IDamageAble>(out var receiver))
            {
                CombatEvent combatEvent = new CombatEvent
                {
                    Sender = sender,
                    Receiver = receiver,
                    Damage = damage,
                    UseEffect = true,
                    EffectName = "ArrowHit",
                    EffectPosition = receiver.GameObject.transform.position,
                    NetworkObject = receiver.NetworkObject
                };

                CombatSystem.Instance.AddCombatEvent(combatEvent);
            }

            Destroy(gameObject);
        }
    }
}
