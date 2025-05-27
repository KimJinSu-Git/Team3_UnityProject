using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class SkillManager : NetworkBehaviour
{
    public static SkillManager Instance;

    [Header("Spell Prefabs")]
    public GameObject arrowRainPrefab;
    public GameObject fireballPrefab;
    
    public Transform hostKingTower;
    public Transform clientKingTower;
    
    [HideInInspector]
    public Transform myKingTowerTransform;

    private void Awake()
    {
        Instance = this;
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

    public void CastSkill(SkillData data, Vector3 position)
    {
        if (!Runner.IsRunning) return;

        PlayerRef player = Runner.LocalPlayer;
        GameObject spellPrefab = GetSpellPrefab(data.skillName);

        Runner.Spawn(spellPrefab, position, Quaternion.identity, player,
            (runner, obj) =>
            {
                if (data.skillName == "ArrowRain")
                {
                    obj.GetComponent<ArrowRainSpell>()?.Init(data, position, player, myKingTowerTransform);
                }
                else if (data.skillName == "Fireball")
                {
                    obj.GetComponent<FireballSpell>()?.Init(data, position, player, myKingTowerTransform);
                }
            });
    }

    private GameObject GetSpellPrefab(string skillName)
    {
        switch (skillName)
        {
            case "ArrowRain": return arrowRainPrefab;
            case "Fireball": return fireballPrefab;
            default: return null;
        }
    }
}
