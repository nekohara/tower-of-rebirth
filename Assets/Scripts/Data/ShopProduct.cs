public enum ShopProductType
{
    Item,
    Weapon,
    Armor
}

[System.Serializable]
public class ShopProduct
{
    public ShopProductType productType;

    public InventoryItem item;
    public Weapon weapon;
    public Armor armor;

    public string ProductName {
        get {
            return productType switch
            {
                ShopProductType.Item =>
                    item != null ? item.name : string.Empty,

                ShopProductType.Weapon =>
                    weapon != null ? weapon.name : string.Empty,

                ShopProductType.Armor =>
                    armor != null ? armor.name : string.Empty,

                _ => string.Empty
            };
        }
    }

    public int Price {
        get {
            return productType switch
            {
                ShopProductType.Item =>
                    item != null ? item.price : 0,

                ShopProductType.Weapon =>
                    weapon != null ? weapon.price : 0,

                ShopProductType.Armor =>
                    armor != null ? armor.price : 0,

                _ => 0
            };
        }
    }
}