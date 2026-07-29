using System;

[Serializable]
public class Skill
{
    public string id;
    public string name;

    public int powerMultiplier;
    public int healAmount;
    public bool isHealSkill;

    public int maxUseCount;
    public int currentUseCount;

    public Skill(
        string id,
        string name,
        int powerMultiplier,
        int healAmount,
        bool isHealSkill,
        int maxUseCount)
    {
        this.id = id;
        this.name = name;
        this.powerMultiplier = powerMultiplier;
        this.healAmount = healAmount;
        this.isHealSkill = isHealSkill;
        this.maxUseCount = maxUseCount;
        currentUseCount = maxUseCount;
    }
}