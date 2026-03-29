using UnityEngine;

[System.Serializable]
public class Weapon
{
    public string name;
    public int price;
    public int power;

    public Weapon(string name, int price, int power)
    {
        this.name = name;
        this.price = price;
        this.power = power;
    }
}