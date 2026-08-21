using TMPro;
using UnityEngine;

public class StatusUI : MonoBehaviour
{
    [SerializeField] private TMP_Text hpText;

    void Update()
    {
        var status = GameManager.Instance.playerStatus;

        hpText.text = $"HP: {status.hp}/{status.maxHp}";

        float rate = (float)status.hp / status.maxHp;

        if (rate > 0.5f)
            hpText.color = Color.white;
        else if (rate > 0.2f)
            hpText.color = Color.yellow;
        else
            hpText.color = Color.red;
    }
}