using UnityEngine;
using TMPro;

public class TownManager : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text levelText;
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
    new Armor("Šv‚ÌŠZ", 12, 5),
    new Armor("“S‚ÌŠZ", 30, 10)
};

    private Weapon[] weapons =
{
    new Weapon("–Ø‚ÌŒ•", 10, 2),
    new Weapon("“S‚ÌŒ•", 25, 4),
    new Weapon("|‚ÌŒ•", 50, 7)
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
            GameManager.Instance.playerStatus.gold -= restCost;
            GameManager.Instance.playerStatus.hp = GetTotalMaxHp();

            if (messageText != null)
            {
                messageText.text = $"{restCost}G•¥‚Á‚Ä‹x‚ñ‚¾B‘Ì—Í‚ª‰ñ•œ‚µ‚½I";
            }
        }
        else
        {
            if (messageText != null)
            {
                messageText.text = "‚¨‹à‚ª‘«‚è‚È‚¢c";
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

            messageText.text = "ƒ|[ƒVƒ‡ƒ“‚ğw“ü‚µ‚½I";
        }
        else
        {
            messageText.text = "‚¨‹à‚ª‘«‚è‚È‚¢c";
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

            messageText.text = $"{weapon.name}‚ğ‘•”õ‚µ‚½I";
        }
        else
        {
            messageText.text = "‚¨‹à‚ª‘«‚è‚È‚¢c";
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

    public void OnClickDungeon()
    {
        SceneLoader.Instance.LoadDungeon();
    }

    private void BuyArmor(Armor armor)
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.playerGold >= armor.price)
        {
            GameManager.Instance.playerGold -= armor.price;
            GameManager.Instance.armorName = armor.name;
            GameManager.Instance.armorDefense = armor.defense;

            int newMaxHp = GameManager.Instance.playerStatus.level * 0; // g‚í‚È‚¢‚Ì‚Å–³‹‚µ‚ÄOK
            int oldMaxHp = GameManager.Instance.playerStatus.maxHp;
            int totalMaxHp = GetTotalMaxHp();

            // ‘•”õ‚µ‚½uŠÔ‚ÉŒ»İHP‚àãŒÀ“à‚Å’²®
            if (GameManager.Instance.playerStatus.hp > totalMaxHp)
            {
                GameManager.Instance.playerStatus.hp = totalMaxHp;
            }

            messageText.text = $"{armor.name}‚ğ‘•”õ‚µ‚½IÅ‘åHPƒAƒbƒvI";
        }
        else
        {
            messageText.text = "‚¨‹à‚ª‘«‚è‚È‚¢c";
        }

        RefreshUI();
    }

    private int GetTotalMaxHp()
    {
        if (GameManager.Instance == null) return 20;

        return GameManager.Instance.playerStatus.maxHp;
    }
    private void RefreshUI()
    {
        if (GameManager.Instance == null) return;

        levelText.text = $"Lv: {GameManager.Instance.playerStatus.level}";
        goldText.text = $"Gold: {GameManager.Instance.playerStatus.gold}";
        weaponText.text = $"•Ší: {GameManager.Instance.weaponName} (+{GameManager.Instance.weaponPower})";
        armorText.text = $"–h‹ï: {GameManager.Instance.armorName} (+Defense   {GameManager.Instance.armorDefense})";
        potionText.text = $"Potion: {GameManager.Instance.potionCount}";
    }

}