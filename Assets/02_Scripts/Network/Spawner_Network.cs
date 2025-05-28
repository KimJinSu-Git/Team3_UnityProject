using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;

public class Spawner_Network : NetworkBehaviour
{
    public Transform Area;

    public static Spawner_Network Instance;

    public void Start()
    {
        Instance = this;
    }

    // public void PoolCreate()
    // {
    //     foreach (var monsterData in CardHandManager.Instance.monsterDatas)
    //     {
    //         GameObject prefab = monsterData.Value.prefab;
    //
    //         NetworkObject networkObject;
    //         // if (prefab.TryGetComponent<>(out networkObject) == true) // 네트워크 오브젝트가 있을때만
    //         // {
    //         //     //NetworkObjectPool.De
    //         // }
    //     }
    // }
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)] // 서버에서 실행
    public void RPC_SpawnMonster(string prefabName, Vector3 spawnPos, Quaternion spawnRot, PlayerRef player)
    {
        if (Object.HasStateAuthority == false) return;
        
        NetworkObject networkMonster;

        CardHandManager.Instance.monsterDatas.TryGetValue(prefabName, out MonsterData monsterData);
        //Vector3 WorldSpawnPos = Area.TransformPoint(spawnPos);
        Debug.Log(prefabName);
        if (Object.HasStateAuthority) // 내가 클라면
        {
            Runner.Spawn(monsterData.prefab, spawnPos, spawnRot, player,
                onBeforeSpawned: (runner, obj) =>
                {
                    obj.GetComponent<BaseMonsterController>().playerRef = player;
                    // obj.transform.SetParent(Area);
                    // obj.transform.localPosition = spawnPos;
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
    
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    public void RPC_SpawnSpell(string skillName, Vector3 spawnPos, PlayerRef player)
    {
        SkillData skillData = SkillManager.Instance.GetSkillData(skillName);
        GameObject spellPrefab = SkillManager.Instance.GetSpellPrefab(skillName);

        if (spellPrefab == null || skillData == null)
        {
            Debug.LogError($"[RPC_SpawnSpell] ❌ Spell prefab not found on this client: {skillName}");
            return;
        }

        Runner.Spawn(spellPrefab, spawnPos, Quaternion.identity, player,
            onBeforeSpawned: (runner, obj) =>
            {
                if (obj.TryGetComponent(out ArrowRainSpell arrowRain))
                {
                    arrowRain.Init(skillData, spawnPos, player, GetKingTower(player));
                }
                else if (obj.TryGetComponent(out FireballSpell fireball))
                {
                    fireball.Init(skillData, spawnPos, player, GetKingTower(player));
                }
            });
    }

    public void RequestSpawnSpell(string skillName, Vector3 position)
    {
        if (!Runner.IsRunning) return;
        RPC_SpawnSpell(skillName, position, Runner.LocalPlayer);
    }

    private Transform GetKingTower(PlayerRef player)
    {
        return SkillManager.Instance.GetKingTowerOf(player);
    }
}
