using UnityEngine;
using TMPro;

public class TownManager : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private int restCost = 5;
    [SerializeField] private int potionPrice = 5;
    [SerializeField] private TMP_Text potionText;

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
            GameManager.Instance.playerHp = GameManager.Instance.maxHp;

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

    private void RefreshUI()
    {
        if (GameManager.Instance == null) return;

        levelText.text = $"Lv: {GameManager.Instance.playerLevel}";
        hpText.text = $"HP: {GameManager.Instance.playerHp}/{GameManager.Instance.maxHp}";
        hpText.color = GameManager.Instance.playerHp < 10 ? Color.red : Color.white;
        goldText.text = $"Gold: {GameManager.Instance.playerGold}";
        potionText.text = $"Potion: {GameManager.Instance.potionCount}";
    }
}