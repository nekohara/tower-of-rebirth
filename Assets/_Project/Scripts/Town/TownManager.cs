using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("ショップパネル")]
    [SerializeField] private GameObject mainCommandPanel;
    [SerializeField] private GameObject weaponShopPanel;
    [SerializeField] private GameObject armorShopPanel;
    [SerializeField] private GameObject itemShopPanel;
    [SerializeField]
    private ShopPanelController itemShopController;

    [SerializeField]
    private ShopPanelController weaponShopController;

    [SerializeField]
    private ShopPanelController armorShopController;

    [Header("効果音")]
    [SerializeField]
    private AudioSource seAudioSource;

    [SerializeField]
    private AudioClip innSe;

    private void Start()
    {
        ShowMainCommands();
        RefreshUI();
    }

    public void Rest()
    {
        if (GameManager.Instance == null)
            return;

        if (GameManager.Instance.playerStatus.gold >= restCost)
        {
            GameManager.Instance.playerStatus.gold -= restCost;
            GameManager.Instance.playerStatus.hp = GetTotalMaxHp();

            foreach (Skill skill in GameManager.Instance.playerStatus.skills)
            {
                skill.currentUseCount = skill.maxUseCount;
            }

            PlayInnSe();

            if (messageText != null)
            {
                messageText.text =
                    $"{restCost}G払って休んだ。体力とスキルが回復した！";
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


    public void OnClickDungeon()
    {
        if (SceneLoader.Instance == null)
            return;

        SceneLoader.Instance.LoadDungeonFromTown();
    }

    private int GetTotalMaxHp()
    {
        if (GameManager.Instance == null) return 20;

        return GameManager.Instance.playerStatus.maxHp;
    }
    public void RefreshUI()
    {
        GameManager gm = GameManager.Instance;

        if (gm == null)
            return;

        if (goldText != null)
            goldText.text = $"Gold: {gm.playerStatus.gold}";

        if (weaponText != null)
            weaponText.text =
                $"武器: {gm.weaponName} (+{gm.weaponPower})";

        if (armorText != null)
            armorText.text =
                $"防具: {gm.armorName} (+{gm.armorDefense})";

        if(potionText != null)
{
            InventoryItem potion =
                gm.playerStatus.inventory.Find(
                    item => item.id == "potion");

            int potionCount = potion?.count ?? 0;

            potionText.text = $"Potion: {potionCount}";
        }
    }

    public void OnClickStatus()
    {
        StatusManager.OpenStatus();
    }


    public void OpenItemShop()
    {
        mainCommandPanel.SetActive(false);
        itemShopPanel.SetActive(true);
        weaponShopPanel.SetActive(false);
        armorShopPanel.SetActive(false);
        itemShopController.CreateProductButtons();

        SetMessage("道具屋へようこそ。");
    }

    public void OpenWeaponShop()
    {
        mainCommandPanel.SetActive(false);
        itemShopPanel.SetActive(false);
        weaponShopPanel.SetActive(true);
        armorShopPanel.SetActive(false);
        weaponShopController.CreateProductButtons();


        SetMessage("武器屋へようこそ。");
    }

    public void OpenArmorShop()
    {
        mainCommandPanel.SetActive(false);
        itemShopPanel.SetActive(false);
        weaponShopPanel.SetActive(false);
        armorShopPanel.SetActive(true);
        armorShopController.CreateProductButtons();

        SetMessage("防具屋へようこそ。");
    }

    public void ShowMainCommands()
    {
        mainCommandPanel.SetActive(true);
        itemShopPanel.SetActive(false);
        weaponShopPanel.SetActive(false);
        armorShopPanel.SetActive(false);
    }

    public void SetMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;
    }

    private void PlayInnSe()
    {
        if (seAudioSource == null ||
            innSe == null)
        {
            return;
        }

        seAudioSource.PlayOneShot(innSe);
    }
}