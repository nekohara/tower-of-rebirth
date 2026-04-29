using TMPro;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;
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
        RefreshPlayerUI();
    }

    private void RefreshPlayerUI()
    {
        if (GameManager.Instance != null)
        {
            levelText.text = $"Lv: {GameManager.Instance.playerStatus.level}";
            expText.text = $"EXP: {GameManager.Instance.playerExp}/{GameManager.Instance.nextExp}";
            potionText.text = $"Potion: {GameManager.Instance.potionCount}";
            weaponText.text = $"ïêäÌ: {GameManager.Instance.weaponName} (+{GameManager.Instance.weaponPower})";
            armorText.text = $"ñhãÔ: {GameManager.Instance.armorName} (+HP {GameManager.Instance.armorHpBonus})";
        }

        dungeonNameText.text = $"{dungeonName}:{floor}F";
    }


    private int GetTotalMaxHp()
    {
        if (GameManager.Instance == null) return 20;

        return GameManager.Instance.playerStatus.maxHp + GameManager.Instance.armorHpBonus;
    }

}
