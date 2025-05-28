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
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)] // 서버에서 실행
    public void RPC_SpawnMonster(string prefabName, Vector3 spawnPos, Quaternion spawnRot, PlayerRef player)
    {
        if (Object.HasStateAuthority == false) return;
        
        NetworkObject networkMonster;

        CardHandManager.Instance.cardData.TryGetValue(prefabName, out BaseData baseData);
        Debug.Log(prefabName);
        if (Object.HasStateAuthority)
        {
            Runner.Spawn(baseData.prefab, spawnPos, spawnRot, player,
                onBeforeSpawned: (runner, obj) =>
                {
                    obj.GetComponent<BaseMonsterController>().playerRef = player;
                });
        }
        Debug.Log(player.PlayerId);
    }
    
    public void RequestSpawn(string prefabName, Vector3 position, Quaternion rotation)
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
        // 스킬 데이터를 CardHandManager에서 가져오기
        //SkillData skillData = null;
        BaseData baseData = null;
        GameObject spellPrefab = null;

        // CardHandManager의 스킬 데이터에서 찾기
        if (CardHandManager.Instance.cardData.TryGetValue(skillName, out baseData))
        {
            spellPrefab = baseData.prefab; // SkillData에 prefab 필드가 있다고 가정
        }
        if (baseData is SkillData spell)
        {
            // 백업으로 SkillManager에서도 찾기
            if (baseData == null)
            {
                baseData = SkillManager.Instance.GetSkillData(skillName);
                spellPrefab = SkillManager.Instance.GetSpellPrefab(skillName);
            }

            if (spellPrefab == null || baseData == null)
            {
                Debug.LogError($"[RPC_SpawnSpell] ❌ Spell prefab not found: {skillName}");
                return;
            }

            Runner.Spawn(spellPrefab, spawnPos, Quaternion.identity, player, onBeforeSpawned: (runner, obj) =>
            {
                if (obj.TryGetComponent(out ArrowRainSpell arrowRain))
                {
                    arrowRain.Init(spell, spawnPos, player, GetKingTower(player));
                }
                else if (obj.TryGetComponent(out FireballSpell fireball))
                {
                    fireball.Init(spell, spawnPos, player, GetKingTower(player));
                }
                // 다른 스킬 타입들도 여기에 추가 가능
            });
        }
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