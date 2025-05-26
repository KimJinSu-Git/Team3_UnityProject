using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StatCalculator
{
    public static int GetHP(MonsterData_Mainmenu monster, int level)
    {
        return Mathf.RoundToInt(monster.baseHP * (1 + 0.15f * (level - 1)));
    }

    public static float GetDamage(MonsterData_Mainmenu monster, int level)
    {
        return monster.baseDamage * (1 + 0.12f * (level - 1));
    }

    public static float GetAttackSpeed(MonsterData_Mainmenu monster, int level)
    {
        return monster.baseAttackSpeed;
    }
}
