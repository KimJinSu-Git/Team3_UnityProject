using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;

public class Spawner_Network : NetworkBehaviour
{
    public Transform Area;
    public void PoolCreate()
    {
        foreach (var monsterData in CardHandManager.Instance.monsterDatas)
        {
            GameObject prefab = monsterData.Value.prefab;

            NetworkObject networkObject;
            // if (prefab.TryGetComponent<>(out networkObject) == true) // 네트워크 오브젝트가 있을때만
            // {
            //     //NetworkObjectPool.De
            // }
        }
    }
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)] // 서버에서 실행
    public void RPC_SpawnMonster(string prefabName, Vector3 spawnPos, Quaternion spawnRot, PlayerRef player)
    {
        if (Object.HasStateAuthority == false) return;
        
        NetworkObject networkMonster;

        CardHandManager.Instance.monsterDatas.TryGetValue(prefabName, out MonsterData monsterData);
        Vector3 WorldSpawnPos = Area.TransformPoint(spawnPos);
        Debug.Log(prefabName);
        if (Object.HasStateAuthority) // 내가 클라면
        {
            Runner.Spawn(monsterData.prefab, WorldSpawnPos, spawnRot, player,
                onBeforeSpawned: (runner, obj) =>
                {
                    obj.GetComponent<BaseMonsterContorller>().playerRef = player;
                    obj.transform.SetParent(Area);
                });
        }
        Debug.Log(player.PlayerId);
    }
    public void RequestSpawn(string prefabName,Vector3 position, Quaternion rotation)
    {
        if (!Runner.IsRunning) return;
        // 요청한 플레이어
        PlayerRef localPlayer = Runner.LocalPlayer;
        // 서버의 RPC를 호출해서 서버에 보냄
        RPC_SpawnMonster(prefabName, position, rotation, localPlayer);
    }
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_RequestTakeDamage(NetworkObject targetMonster, int damage)
    {
        if (!Object.HasStateAuthority) return;
        targetMonster.GetComponent<IDamageAble>().TakeDamage(damage);
    }
}
