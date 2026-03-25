using UnityEngine;
using TMPro;

public class TownManager : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private int restCost = 5;

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

    private void RefreshUI()
    {
        if (GameManager.Instance == null) return;

        levelText.text = $"Lv: {GameManager.Instance.playerLevel}";
        hpText.text = $"HP: {GameManager.Instance.playerHp}/{GameManager.Instance.maxHp}";
        hpText.color = GameManager.Instance.playerHp < 10 ? Color.red : Color.white;
        goldText.text = $"Gold: {GameManager.Instance.playerGold}";
    }
}