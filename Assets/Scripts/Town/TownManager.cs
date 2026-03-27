using UnityEngine;
using TMPro;

public class TownManager : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text weaponText;
    [SerializeField] private int restCost = 5;
    [SerializeField] private int potionPrice = 5;
    [SerializeField] private TMP_Text potionText;
    [SerializeField] private int weaponPrice = 10;
    [SerializeField] private int weaponPowerValue = 2;

    [SerializeField] private TMP_Text armorText;

    private Armor[] armors =
    {
    new Armor("革の鎧", 12, 5),
    new Armor("鉄の鎧", 30, 10)
};

    private Weapon[] weapons =
{
    new Weapon("木の剣", 10, 2),
    new Weapon("鉄の剣", 25, 4),
    new Weapon("鋼の剣", 50, 7)
};
    private void Start()
    {
        RefreshUI();
    }

    public void Rest()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.playerGold >= restCost)
        {
            GameManager.Instance.playerGold -= restCost;
            GameManager.Instance.playerHp = GetTotalMaxHp();

            if (messageText != null)
            {
                messageText.text = $"{restCost}G払って休んだ。体力が回復した！";
            }
        }
        else
        {
            if (messageText != null)
            {
                messageText.text = "お金が足りない…";
            }
        }

        RefreshUI();
    }

    public void BuyPotion()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.playerGold >= potionPrice)
        {
            GameManager.Instance.playerGold -= potionPrice;
            GameManager.Instance.potionCount++;

            messageText.text = "ポーションを購入した！";
        }
        else
        {
            messageText.text = "お金が足りない…";
        }

        RefreshUI();
    }

    public void BuyWoodSword()
    {
        BuyWeapon(weapons[0]);
    }

    public void BuyIronSword()
    {
        BuyWeapon(weapons[1]);
    }


    public void BuyWeapon(Weapon weapon)
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.playerGold >= weapon.price)
        {
            GameManager.Instance.playerGold -= weapon.price;
            GameManager.Instance.weaponPower = weapon.power;
            GameManager.Instance.weaponName = weapon.name;

            messageText.text = $"{weapon.name}を装備した！";
        }
        else
        {
            messageText.text = "お金が足りない…";
        }

        RefreshUI();
    }

    public void BuyLeatherArmor()
    {
        BuyArmor(armors[0]);
    }

    public void BuyIronArmor()
    {
        BuyArmor(armors[1]);
    }

    private void BuyArmor(Armor armor)
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.playerGold >= armor.price)
        {
            GameManager.Instance.playerGold -= armor.price;
            GameManager.Instance.armorName = armor.name;
            GameManager.Instance.armorHpBonus = armor.hpBonus;

            int newMaxHp = GameManager.Instance.playerLevel * 0; // 使わないので無視してOK
            int oldMaxHp = GameManager.Instance.maxHp;
            int totalMaxHp = GetTotalMaxHp();

            // 装備した瞬間に現在HPも上限内で調整
            if (GameManager.Instance.playerHp > totalMaxHp)
            {
                GameManager.Instance.playerHp = totalMaxHp;
            }

            messageText.text = $"{armor.name}を装備した！最大HPアップ！";
        }
        else
        {
            messageText.text = "お金が足りない…";
        }

        RefreshUI();
    }

    private int GetTotalMaxHp()
    {
        if (GameManager.Instance == null) return 20;

        return GameManager.Instance.maxHp + GameManager.Instance.armorHpBonus;
    }
    private void RefreshUI()
    {
        if (GameManager.Instance == null) return;

        levelText.text = $"Lv: {GameManager.Instance.playerLevel}";
        hpText.text = $"HP: {GameManager.Instance.playerHp}/{GetTotalMaxHp()}";
        goldText.text = $"Gold: {GameManager.Instance.playerGold}";
        weaponText.text = $"武器: {GameManager.Instance.weaponName} (+{GameManager.Instance.weaponPower})";
        armorText.text = $"防具: {GameManager.Instance.armorName} (+HP {GameManager.Instance.armorHpBonus})";
        potionText.text = $"Potion: {GameManager.Instance.potionCount}";
    }

}