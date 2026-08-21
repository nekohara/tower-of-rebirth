using UnityEngine;

[System.Serializable]
public class Armor
{
    public string name;
    public int price;
    public int defense;

    public Armor(string name, int price, int defense)
    {
        this.name = name;
        this.price = price;
        this.defense = defense;
    }
}