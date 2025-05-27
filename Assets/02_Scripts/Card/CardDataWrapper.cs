[System.Serializable]
public class CardDataWrapper
{
    public enum CardType { Monster, Skill }
    public CardType cardType;

    public MonsterData_Mainmenu monsterData;
    public SkillData skillData;

    public bool IsMonster => monsterData != null;
    public bool IsSkill => skillData != null;
}