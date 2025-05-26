using System.Collections;
using UnityEngine;
using Fusion;

public class ArrowRainProjectile : MonoBehaviour
{
    private Vector3 startPos;
    private Vector3 targetPos;
    private PlayerRef caster;
    private int damage;

    private float height = 5f;
    private float duration = 0.5f;

    public void Init(Vector3 targetPos, PlayerRef owner, int damage)
    {
        this.startPos = transform.position;
        this.targetPos = targetPos;
        this.caster = owner;
        this.damage = damage;

        StartCoroutine(MoveToTarget());
    }

    private IEnumerator MoveToTarget()
    {
        float t = 0f;
        Vector3 prevPos = transform.position;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            // 포물선 궤적 계산
            Vector3 flatPos = Vector3.Lerp(startPos, targetPos, t);
            float arc = height * Mathf.Sin(Mathf.PI * t);
            Vector3 pos = new Vector3(flatPos.x, flatPos.y + arc, flatPos.z);
            transform.position = pos;

            // 🔁 이동 방향 기반으로 회전
            Vector3 velocity = (transform.position - prevPos).normalized;
            if (velocity != Vector3.zero)
                transform.forward = velocity;

            prevPos = transform.position;
            yield return null;
        }

        // 착지 후 범위 판정
        Collider[] hits = Physics.OverlapSphere(transform.position, 1f, LayerMask.GetMask("Monster", "Tower"));
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageAble>(out var damageAble) && damageAble.PlayerRef != caster)
            {
                CombatSystem.Instance.AddCombatEvent(new CombatEvent
                {
                    Receiver = damageAble,
                    Damage = damage,
                    UseEffect = true,
                    EffectName = "ArrowHit",
                    EffectPosition = damageAble.GameObject.transform.position,
                    NetworkObject = damageAble.NetworkObject
                });
            }
        }

        Destroy(gameObject);
    }
}