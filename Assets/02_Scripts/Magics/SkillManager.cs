using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

public class SkillManager : NetworkBehaviour
{
    public static SkillManager Instance;

    // [Header("Spell Prefabs")]
    // public GameObject arrowRainPrefab;
    // public GameObject fireballPrefab;
    
    [System.Serializable]
    public struct SkillPrefabEntry
    {
        public string skillName;
        public GameObject prefab;
        public SkillData data;
    }
    
    [Header("스킬 프리팹 테이블")]
    public List<SkillPrefabEntry> spellPrefabs; // name + prefab

    private Dictionary<string, GameObject> spellPrefabDict;
    
    [Header("킹타워 Transform")]
    public Transform hostKingTower;
    public Transform clientKingTower;
    
    [HideInInspector]
    public Transform myKingTowerTransform;

    private void Awake()
    {
        Instance = this;
        spellPrefabDict = new Dictionary<string, GameObject>();
        foreach (var entry in spellPrefabs)
        {
            if (!spellPrefabDict.ContainsKey(entry.skillName))
                spellPrefabDict.Add(entry.skillName, entry.prefab);
        }
    }
    
    private void Start()
    {
        var runner = FindObjectOfType<NetworkRunner>();
        if (runner.IsServer)
        {
            myKingTowerTransform = hostKingTower;
        }
        else
        {
            myKingTowerTransform = clientKingTower;
        }
    }
    
    public GameObject GetSpellPrefab(string skillName)
    {
        return spellPrefabDict.TryGetValue(skillName, out var prefab) ? prefab : null;
    }
    
    // 플레이어 기준 킹타워 반환
    public Transform GetKingTowerOf(PlayerRef player)
    {
        var runner = FindObjectOfType<NetworkRunner>();
        
        if (runner.IsServer)
        {
            return player == runner.LocalPlayer ? hostKingTower : clientKingTower;
        }
        else
        {
            return player == runner.LocalPlayer ? clientKingTower : hostKingTower;
        }
    }

    public void CastSkill(string skillName, Vector3 targetPosition)
    {
        Spawner_Network.Instance.RequestSpawnSpell(skillName, targetPosition);
    }
    
    public SkillData GetSkillData(string skillName)
    {
        return spellPrefabs.FirstOrDefault(x => x.skillName == skillName).data;
    }

    // public void CastSkill(SkillData data, Vector3 position)
    // {
    //     if (!Runner.IsRunning) return;
    //
    //     PlayerRef player = Runner.LocalPlayer;
    //     GameObject spellPrefab = GetSpellPrefab(data.skillName);
    //
    //     Runner.Spawn(spellPrefab, position, Quaternion.identity, player,
    //         (runner, obj) =>
    //         {
    //             if (data.skillName == "ArrowRain")
    //             {
    //                 obj.GetComponent<ArrowRainSpell>()?.Init(data, position, player, myKingTowerTransform);
    //             }
    //             else if (data.skillName == "Fireball")
    //             {
    //                 obj.GetComponent<FireballSpell>()?.Init(data, position, player, myKingTowerTransform);
    //             }
    //         });
    // }
    //
    // private GameObject GetSpellPrefab(string skillName)
    // {
    //     switch (skillName)
    //     {
    //         case "ArrowRain": return arrowRainPrefab;
    //         case "Fireball": return fireballPrefab;
    //         default: return null;
    //     }
    // }
}
