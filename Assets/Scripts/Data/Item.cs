using System;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    public string id;
    public string name;
    public int count;

    public float weight;

    public int healAmount;
    public bool isHealItem;
    public int price;
}