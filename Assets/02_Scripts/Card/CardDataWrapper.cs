[System.Serializable]
public class CardDataWrapper
{
    public enum CardType { Monster, Skill }

    public CardType cardType;

    public MonsterData monsterData;
    public SkillData skillData;

    public bool IsMonster => cardType == CardType.Monster && monsterData != null;
    public bool IsSkill => cardType == CardType.Skill && skillData != null;
}