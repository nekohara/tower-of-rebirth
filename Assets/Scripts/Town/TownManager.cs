using UnityEngine;
using TMPro;

public class TownManager : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text hpText;

    private void Start()
    {
        RefreshUI();
    }

    public void Rest()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.playerHp = 20;
        }

        if (messageText != null)
        {
            messageText.text = "‘Ì—Í‚ª‰ñ•œ‚µ‚½I";
        }

        Debug.Log("HP‰ñ•œ");

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (GameManager.Instance == null) return;

        levelText.text = $"Lv: {GameManager.Instance.playerLevel}";
        hpText.text = $"HP: {GameManager.Instance.playerHp}/{GameManager.Instance.maxHp}";
        hpText.color = GameManager.Instance.playerHp < 10 ? Color.red : Color.white;
    }
}