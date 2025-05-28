using System.Collections;
using Fusion;
using UnityEngine;

/// <summary>
/// 포물선으로 날아가 착지 후 폭발하는 화염구 발사체
/// </summary>

public class FireballProjectile : NetworkBehaviour
{
    private Vector3 startPos;
    private Vector3 targetPos;
    private PlayerRef caster;
    private int damage;

    [Header("Settings")]
    public float height = 5f;
    public float duration = 0.6f;
    public float explosionRadius = 3f;

    public void Init(Vector3 targetPos, PlayerRef owner, int damage)
    {
        if (Object.HasStateAuthority)
        {
            Vector3 start = transform.position;
            RPC_Start(start, targetPos, owner, damage);
        }
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    private void RPC_Start(Vector3 start, Vector3 target, PlayerRef owner, int dmg)
    {
        this.startPos = start;
        this.targetPos = target;
        this.caster = owner;
        this.damage = dmg;

        StartCoroutine(MoveToTarget());
    }

    private IEnumerator MoveToTarget()
    {
        float t = 0f;
        Vector3 prevPos = transform.position;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            Vector3 flatPos = Vector3.Lerp(startPos, targetPos, t);
            float arc = height * Mathf.Sin(Mathf.PI * t);
            Vector3 pos = new Vector3(flatPos.x, flatPos.y + arc, flatPos.z);
            transform.position = pos;

            Vector3 velocity = (transform.position - prevPos).normalized;
            if (velocity != Vector3.zero)
                transform.forward = velocity;

            prevPos = transform.position;
            yield return null;
        }

        Collider[] hits = Physics.OverlapSphere(targetPos, explosionRadius, LayerMask.GetMask("Monster", "Tower"));
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageAble>(out var damageAble) && damageAble.PlayerRef != caster)
            {
                CombatSystem.Instance.AddCombatEvent(new CombatEvent
                {
                    Receiver = damageAble,
                    Damage = damage,
                    UseEffect = true,
                    EffectName = "Fireball",
                    EffectPosition = damageAble.GameObject.transform.position,
                    NetworkObject = damageAble.NetworkObject
                });
            }
        }

        if (Object != null && Object.IsValid && Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
        }
    }
}

// public class FireballProjectile : NetworkBehaviour
// {
//     private Vector3 startPos;
//     private Vector3 targetPos;
//     private PlayerRef caster;
//     private int damage;
//
//     [Header("Settings")]
//     public float height = 5f;
//     public float duration = 0.6f;
//     public float explosionRadius = 3f;
//
//     public void Init(Vector3 targetPos, PlayerRef owner, int damage)
//     {
//         this.startPos = transform.position;
//         this.targetPos = targetPos;
//         this.caster = owner;
//         this.damage = damage;
//
//         StartCoroutine(MoveToTarget());
//     }
//
//     private IEnumerator MoveToTarget()
//     {
//         float t = 0f;
//         Vector3 prevPos = transform.position;
//
//         while (t < 1f)
//         {
//             t += Time.deltaTime / duration;
//
//             Vector3 flatPos = Vector3.Lerp(startPos, targetPos, t);
//             float arc = height * Mathf.Sin(Mathf.PI * t);
//             Vector3 pos = new Vector3(flatPos.x, flatPos.y + arc, flatPos.z);
//             transform.position = pos;
//
//             Vector3 velocity = (transform.position - prevPos).normalized;
//             if (velocity != Vector3.zero)
//                 transform.forward = velocity;
//
//             prevPos = transform.position;
//             yield return null;
//         }
//
//         // 범위 피해 처리
//         Collider[] hits = Physics.OverlapSphere(targetPos, explosionRadius, LayerMask.GetMask("Monster", "Tower"));
//         foreach (var hit in hits)
//         {
//             if (hit.TryGetComponent<IDamageAble>(out var damageAble) && damageAble.PlayerRef != caster)
//             {
//                 CombatSystem.Instance.AddCombatEvent(new CombatEvent
//                 {
//                     Receiver = damageAble,
//                     Damage = damage,
//                     UseEffect = true,
//                     EffectName = "Fireball",
//                     EffectPosition = damageAble.GameObject.transform.position,
//                     NetworkObject = damageAble.NetworkObject
//                 });
//             }
//         }
//
//         Destroy(gameObject);
//     }
// }