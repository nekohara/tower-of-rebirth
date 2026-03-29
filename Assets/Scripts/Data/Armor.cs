using UnityEngine;

[System.Serializable]
public class Armor
{
    public string name;
    public int price;
    public int hpBonus;

    public Armor(string name, int price, int hpBonus)
    {
        this.name = name;
        this.price = price;
        this.hpBonus = hpBonus;
    }
}