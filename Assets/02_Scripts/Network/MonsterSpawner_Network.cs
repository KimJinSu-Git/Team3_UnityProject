using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class MonsterSpawner_Network : NetworkBehaviour
{
    public NetworkObject monsterPrefab;

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)] // 서버에서 실행
    public void RPC_SpawnMonster(Vector3 spawnPos, Quaternion spawnRot, PlayerRef player)
    {
        if (Object.HasStateAuthority == false) return;

        NetworkObject networkMonster = Runner.Spawn(monsterPrefab, Vector3.zero, Quaternion.identity, player);
        //PlayerRef OwnerPlayer;
        //networkMonster.GetComponent<>().OwnerPlayer = player;
    }

    public void RequestSpawn(Vector3 position, Quaternion rotation)
    {
        if (!Runner.IsRunning) return;
        // 요청한 플레이어
        PlayerRef localPlayer = Runner.LocalPlayer;

        // 서버의 RPC를 호출해서 서버에 보낸다
        RPC_SpawnMonster(position, rotation, localPlayer);
    }
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_RequestTakeDamage(NetworkObject targetMonster, int damage)
    {
        if (!Object.HasStateAuthority) return;
        //GetComponent<Monster>().TakeDamage(damage);
    }
}
