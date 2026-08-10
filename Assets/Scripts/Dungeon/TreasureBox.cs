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

    [SerializeField] private GameObject closedModel;
    [SerializeField] private GameObject openedModel;

    public void Open()
    {
        if (isOpened) return;
        //isOpened = true;

        StartCoroutine(OpenRoutine());
    }

    private void Awake()
    {
        if (closedModel != null)
        {
            closedModel.SetActive(true);
        }

        if (openedModel != null)
        {
            openedModel.SetActive(false);
        }
    }

    public void SetTreasureType(TreasureType type)
    {
        treasureType = type;
    }

    IEnumerator OpenRoutine()
    {
        if (isOpened)
        {
            yield break;
        }

        isOpened = true;

        if (messageText != null)
        {
            messageText.text = "宝箱を開けた……";
        }

        if (closedModel != null)
        {
            closedModel.SetActive(false);
        }

        if (openedModel != null)
        {
            openedModel.SetActive(true);
        }

        yield return new WaitForSeconds(0.5f);


        // 中身判定
        int luck = GameManager.Instance.playerStatus.luck;
        int rareRate = Mathf.Min(30, 5 + luck);


        // ★レア判定
        bool isRare = Random.Range(0, 100) < rareRate;


        switch (treasureType)
        {
            case TreasureType.Potion:
                {
                    int count = Random.Range(1, 4);

                    PlayerStatus status =
                        GameManager.Instance.playerStatus;

                    InventoryItem potion =
                        status.inventory.Find(item => item.id == "potion");

                    if (potion == null)
                    {
                        if (messageText != null)
                        {
                            messageText.text =
                                "ポーションのアイテム情報が見つからない。";
                        }

                        Debug.LogError(
                            "inventoryにidがpotionのアイテムがありません。");

                        break;
                    }

                    if (!status.CanAddItem(potion, count))
                    {
                        if (messageText != null)
                        {
                            messageText.text =
                                "荷物が重すぎてポーションを持てない……";
                        }

                        break;
                    }

                    potion.count += count;

                    if (messageText != null)
                    {
                        messageText.text =
                            $"ポーションを{count}個手に入れた！";
                    }

                    break;
                }
            case TreasureType.Gold:
                
                int baseGold = Random.Range(10, 100) + luck * 2;


                int gold = isRare ? baseGold * 3 : baseGold;

                if (isRare)
                {
                    messageText.text = $"レア宝箱！{gold}ゴールドを手に入れた！";
                }
                else
                {
                    string[] messages =
                     {
                        $"{gold}ゴールドを手に入れた！",
                        $"{gold}Gゲット！",
                        $"{gold}ゴールドを発見！"
                    };

                    messageText.text = messages[Random.Range(0, messages.Length)];
                }
                GameManager.Instance.playerStatus.gold += gold;
                break;
            case TreasureType.Trap:
                int damage = Random.Range(1, 10)
                            - GameManager.Instance.playerStatus.GetDefensePower(GameManager.Instance.armorDefense)
                            - luck / 2;

                damage = Mathf.Max(0, damage);
                messageText.text = $"罠だ！{damage}ダメージを受けた！";
                GameManager.Instance.playerStatus.hp -= damage;
                break;
        }

        yield return new WaitForSeconds(0.3f);
     
    }
}