using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathVFXPool : MonoBehaviour
{
    public static DeathVFXPool Instance { get; private set; }

    [Header("VFX Prefab")]
    [SerializeField] private GameObject deathVFXPrefab;

    [Header("초기 풀 크기")]
    [SerializeField] private int initialPoolSize = 10;

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // 초기 풀 생성
        for (int i = 0; i < initialPoolSize; i++)
        {
            
            var obj = Instantiate(deathVFXPrefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }


    public void Spawn(Vector3 position)
    {
        GameObject obj;
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            // 부족할 때는 새로 생성
            obj = Instantiate(deathVFXPrefab, transform);
        }

        obj.transform.position = position + Vector3.up * 2f;
        obj.SetActive(true);

        // 파티클 시스템 시작
        var ps = obj.GetComponent<ParticleSystem>();
        if (ps != null)
            ps.Play();

        // 파티클이 끝난 뒤 다시 풀로 반환
        StartCoroutine(ReturnToPoolAfter(ps, obj));
    }

    private IEnumerator ReturnToPoolAfter(ParticleSystem ps, GameObject obj)
    {
        // 파티클의 예상 재생 길이 (Looping이 꺼져 있을 때만)
        float waitTime = ps.main.duration + ps.main.startLifetime.constantMax;
        yield return new WaitForSeconds(waitTime);

        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}