using System.Collections;
using TMPro;
using UnityEngine;

public class TreasureBox : MonoBehaviour
{
    public enum TreasureType
    {
        Potion,
        Gold,
        Trap
    }

    public TMP_Text messageText;
    public bool isOpened = false;

    [SerializeField] private TreasureType treasureType;


    public void Open()
    {
        if (isOpened) return;
        //isOpened = true;

        StartCoroutine(OpenRoutine());
    }

    public void SetTreasureType(TreasureType type)
    {
        treasureType = type;
    }

    IEnumerator OpenRoutine()
    {
        if (isOpened) yield break;
        isOpened = true;

        messageText.text = "宝箱を開けた…";

        yield return new WaitForSeconds(0.5f);

        // 中身判定
        int luck = GameManager.Instance.playerStatus.luck;

        switch (treasureType)
        {
            case TreasureType.Potion:
                int cnt = Random.Range(1, 4); // 1～3個くらいが自然
                messageText.text = $"ポーションを{cnt}個手に入れた！";
                GameManager.Instance.potionCount += cnt;
                break;

            case TreasureType.Gold:
                int gold = Random.Range(10, 100) + luck * 2;
                messageText.text = $"{gold}ゴールドを手に入れた！";
                GameManager.Instance.playerStatus.gold += gold;
                break;

            case TreasureType.Trap:
                int damage = Random.Range(1, 10)
                            - GameManager.Instance.playerStatus.defense
                            - luck / 2;

                damage = Mathf.Max(0, damage);
                messageText.text = $"罠だ！{damage}ダメージを受けた！";
                GameManager.Instance.playerStatus.hp -= damage;
                break;
        }

        yield return new WaitForSeconds(0.3f);
        Destroy(gameObject);
    }
}