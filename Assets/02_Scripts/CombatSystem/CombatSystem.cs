using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatSystem : MonoBehaviour // MainGame에 CombatSystem 게임오브젝트 만들어서 넣기 
{
    public static CombatSystem Instance;
    public Action<CombatEvent> effectEvent;
    public Dictionary<Collider, IDamageAble> creatureDic = new Dictionary<Collider, IDamageAble>();
    private Queue<CombatEvent> combatEventQueue = new Queue<CombatEvent>();

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        while (combatEventQueue.Count > 0)
        {
            CombatEvent combatEvent = combatEventQueue.Dequeue();

            if (combatEvent.UseEffect == true)
            {
                effectEvent.Invoke(combatEvent);
            }
            combatEvent.Receiver.TakeDamage(combatEvent.Damage, true);

        }
    }

    public void RegisterCreature(Collider collider, IDamageAble damageAble)
    {
        if (creatureDic.ContainsKey(collider) == false)
        {
            creatureDic.TryAdd(collider, damageAble);
        }
    }
    public IDamageAble GetCreatureOrNull(Collider collider)
    {
        if (creatureDic.ContainsKey(collider))
        {
            return creatureDic[collider];
        }
        
        return null;
    }

    public void AddCombatEvent(CombatEvent combatEvent)
    {
        combatEventQueue.Enqueue(combatEvent);
    }
}
