using TMPro;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text playerHpText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text expText;
    [SerializeField] private TMP_Text potionText;
    [SerializeField] private TMP_Text weaponText;
    [SerializeField] private TMP_Text armorText;
    [SerializeField] private TMP_Text dungeonNameText;


    private string dungeonName = "Dungeon";
    private int floor = 1;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        RefreshPlayerUI();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void RefreshPlayerUI()
    {
        if (GameManager.Instance != null)
        {
            playerHpText.text = $"HP: {GameManager.Instance.playerHp}/{GetTotalMaxHp()}";
            playerHpText.color = GameManager.Instance.playerHp <= GetTotalMaxHp() * 0.3f ? Color.red : Color.white;
            levelText.text = $"Lv: {GameManager.Instance.playerLevel}";
            expText.text = $"EXP: {GameManager.Instance.playerExp}/{GameManager.Instance.nextExp}";
            potionText.text = $"Potion: {GameManager.Instance.potionCount}";
            weaponText.text = $"•Ší: {GameManager.Instance.weaponName} (+{GameManager.Instance.weaponPower})";
            armorText.text = $"–h‹ï: {GameManager.Instance.armorName} (+HP {GameManager.Instance.armorHpBonus})";
        }

        dungeonNameText.text = $"{dungeonName}:{floor}F";
    }


    private int GetTotalMaxHp()
    {
        if (GameManager.Instance == null) return 20;

        return GameManager.Instance.maxHp + GameManager.Instance.armorHpBonus;
    }

}
