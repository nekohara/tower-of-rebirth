using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum StatusType
{
    MaxHp,
    Strength,
    Vitality,
    Speed,
    Dexterity,
    Intelligence,
    Luck
}

[System.Serializable]
public class GrowthResult
{
    public bool grew;
    public StatusType type;
    public int amount;

    public GrowthResult(bool grew, StatusType type, int amount = 0)
    {
        this.grew = grew;
        this.type = type;
        this.amount = amount;
    }
}

[System.Serializable]
public class PlayerStatus
{
    public string playerName = "";
    public int level = 1;
    public int maxHp = 20;
    public int hp = 20;
    public int gold = 100;

    public int strength = 3;
    public int vitality = 1;
    public int speed = 5;
    public int dexterity = 1;
    public int intelligence = 1;
    public int luck = 1;

    public string backgroundId = "";
    public string backgroundName = "";

    public GrowthPoints growth = new GrowthPoints();

    public float maxCarryWeight = 30f;

    public List<Skill> skills = new List<Skill>()
{
    new Skill(
        "power_strike",
        "パワーストライク",
        2,
        0,
        false,
        3
    ),
    new Skill(
        "heal",
        "ヒール",
        0,
        15,
        true,
        2
    )
};

    public List<InventoryItem> inventory = new List<InventoryItem>() {
        new InventoryItem{
            id = "potion",
            name = "ポーション",
            count = 3,
            healAmount = 10,
            price = 10,
            isHealItem = true,
            weight=0.5f
            }
        };

    public float GetCurrentWeight()
    {
        return inventory.Sum(item => item.weight * item.count);
    }

    public bool CanAddItem(InventoryItem item, int amount = 1)
    {
        float addedWeight = item.weight * amount;

        return GetCurrentWeight() + addedWeight <= GetMaxCarryWeight() * 1.5f;
    }

    public float GetMaxCarryWeight()
    {
        return maxCarryWeight + strength * 0.5f;
    }


    public int GetEffectiveSpeed()
    {
        float weightRate = GetCurrentWeight() / GetMaxCarryWeight();

        if (weightRate >= 1.25f)
            return Mathf.Max(1, speed - 3);

        if (weightRate >= 1.0f)
            return Mathf.Max(1, speed - 2);

        if (weightRate >= 0.7f)
            return Mathf.Max(1, speed - 1);

        return speed;
    }


    public int GetAttackPower(int weaponPower)
    {
        return strength + weaponPower;
    }

    public int GetDefensePower(int armorPower)
    {
        return vitality + armorPower;
    }

    private int GetRequiredGrowth(int currentStat)
    {
        return 10 + currentStat * 2;
    }

    private int GetRequiredHpGrowth()
    {
        return 10 + maxHp / 2;
    }

    public GrowthResult AddGrowth(StatusType type, int amount)
    {
        switch (type)
        {
            case StatusType.Strength:
                growth.strength += amount;

                if (growth.strength >= GetRequiredGrowth(strength))
                {
                    growth.strength -= GetRequiredGrowth(strength);
                    strength++;
                    return new GrowthResult(true, StatusType.Strength, 1);
                }
                break;

            case StatusType.Vitality:
                growth.vitality += amount;

                if (growth.vitality >= GetRequiredGrowth(vitality))
                {
                    growth.vitality -= GetRequiredGrowth(vitality);
                    vitality++;
                    return new GrowthResult(true, StatusType.Vitality, 1);
                }
                break;

            case StatusType.Speed:
                growth.speed += amount;

                if (growth.speed >= GetRequiredGrowth(speed))
                {
                    growth.speed -= GetRequiredGrowth(speed);
                    speed++;
                    return new GrowthResult(true, StatusType.Speed, 1);
                }
                break;
            case StatusType.MaxHp:
                {
                    int required = GetRequiredHpGrowth();

                    growth.hp += amount;

                    if (growth.hp >= required)
                    {
                        growth.hp -= required;

                        int hpUp = Random.Range(2, 6);
                        maxHp += hpUp;

                        return new GrowthResult(true, StatusType.MaxHp, hpUp);
                    }

                    break;
                }
        }

        return new GrowthResult(false, type);
    }
}

[System.Serializable]
public class GrowthPoints
{
    public int hp;
    public int strength;
    public int vitality;
    public int speed;
    public int dexterity;
    public int intelligence;
    public int luck;
}

