using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    public float speed = 10f;
    private Transform target;
    private int damage;

    public void Init(Transform target, int damage)
    {
        this.target = target;
        this.damage = damage;
        Destroy(gameObject, 2f);
    }

    private void Update()
    {
        if (target == null) return;
        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * (speed * Time.deltaTime);
        transform.LookAt(target);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            IDamageAble target = CombatSystem.Instance.GetCreatureOrNull(other);
            target?.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
