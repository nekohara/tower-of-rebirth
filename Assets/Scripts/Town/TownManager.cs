using UnityEngine;
using TMPro;

public class TownManager : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;

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
    }
}