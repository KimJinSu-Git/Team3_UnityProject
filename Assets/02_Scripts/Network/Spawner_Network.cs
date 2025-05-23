using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;

public class Spawner_Network : NetworkBehaviour
{
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

        CardHandManager.Instance.monsterDatas.TryGetValue(prefabName, out MonsterData monsterData);
        Debug.Log(prefabName);
        // if (Object.HasStateAuthority) // 내가 클라면
        // {
            // NetworkObject networkMonster = Runner.Spawn(monsterData.prefab, spawnPos, spawnRot);
            // networkMonster.GetComponent<BaseMonster>().playerRef = player; // 식별자 세팅
            Runner.Spawn(monsterData.prefab, spawnPos, spawnRot, inputAuthority: null,
                onBeforeSpawned: (runner, obj) =>
                {
                    obj.GetComponent<BaseMonster>().playerRef = player;
                });
        //}
        // else
        // {
        //     NetworkObject networkMonster = Runner.Spawn(monsterData.prefab, -spawnPos, spawnRot);
        //     networkMonster.GetComponent<BaseMonster>().playerRef = player; // 식별자 세팅
        // }
        Debug.Log(player.PlayerId);
    }
    public void RequestSpawn(string prefabName,Vector3 position, Quaternion rotation)
    {
        if (!Runner.IsRunning) return;
        // 요청한 플레이어
        PlayerRef localPlayer = Runner.LocalPlayer;
        //PlayerRef clientOrHost = SessionManager.Instance.PlayerRef;
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
