using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    private class Enemy
    {
        public string name;
        public int hp;
        public int attack;
        public int exp;
        public int gold;

        public Enemy(string name, int hp, int attack, int exp, int gold)
        {
            this.name = name;
            this.hp = hp;
            this.attack = attack;
            this.exp = exp;
            this.gold = gold;
        }
    }

    [SerializeField] private TMP_Text enemyText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text playerHpText;
    [SerializeField] private TMP_Text enemyHpText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text expText;
    [SerializeField] private TMP_Text potionText;

    private int playerHp;
    private int playerAttack;
    private int rewardExp;

    private int enemyHp = 10;
    private int enemyAttack = 2;

    private bool battleEnded = false;

    private Enemy currentEnemy;


    private void Start()
    {
        if (GameManager.Instance != null)
        {
            playerHp = GameManager.Instance.playerHp;
            playerAttack = GameManager.Instance.playerAttack;
        }
        else
        {
            new GameObject("GameManager").AddComponent<GameManager>();
            playerHp = 20; // デバッグ用
            playerAttack = 3;
        }


        var enemies = new Enemy[]
        {
            new Enemy("スライム", 10, 2, 3, 2),
            new Enemy("ゴブリン", 18, 4, 6, 5),
            new Enemy("オオカミ", 12, 6, 5, 4),
        };

        currentEnemy = enemies[Random.Range(0, enemies.Length)];

        enemyHp = currentEnemy.hp;
        enemyAttack = currentEnemy.attack;


        RefreshUI();
        enemyText.text = $"{currentEnemy.name}が現れた！";
        messageText.text = "どうする？";

        Debug.Log("現在HP: " + GameManager.Instance.playerHp);
    }

    public void Fight()
    {
        if (battleEnded) return;

        enemyHp -= playerAttack;

        if (enemyHp <= 0)
        {
            enemyHp = 0;
            RefreshUI();
            WinBattle();
            return;
        }

        playerHp -= enemyAttack;

        if (playerHp <= 0)
        {
            playerHp = 0;
            battleEnded = true;
            RefreshUI();
            messageText.text = "やられてしまった…";
            Invoke(nameof(ReturnToDungeon), 1.5f);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.playerHp = playerHp;
            }
            return;
        }

        RefreshUI();
        // messageText.text = $"{currentEnemy.name}に{playerAttack}ダメージ！\n{currentEnemy.name}の反撃！ {enemyAttack}ダメージ受けた！";
        if (!battleEnded)
        {
            messageText.text = $"{currentEnemy.name}に{playerAttack}ダメージ！\n" + $"{currentEnemy.name}の反撃！{enemyAttack}ダメージ！";
        }

    }

    private void WinBattle()
    {
        battleEnded = true;

        messageText.text = $"{currentEnemy.name}を倒した！\n{currentEnemy.exp}経験値と{currentEnemy.gold}Gを手に入れた！";

        if (GameManager.Instance != null)
        {
            GameManager.Instance.playerHp = playerHp;
            GameManager.Instance.playerExp += currentEnemy.exp;
            GameManager.Instance.playerGold += currentEnemy.gold;
            CheckLevelUp();
        }

        Invoke(nameof(ReturnToDungeon), 2.5f);
    }

    private void CheckLevelUp()
    {
        if (GameManager.Instance == null) return;

        while (GameManager.Instance.playerExp >= GameManager.Instance.nextExp)
        {
            GameManager.Instance.playerExp -= GameManager.Instance.nextExp;
            GameManager.Instance.playerLevel += 1;

            int hpUp = 5;
            int atkUp = 1;

            GameManager.Instance.nextExp += 5;
            GameManager.Instance.maxHp += hpUp;
            GameManager.Instance.playerAttack += atkUp;
            GameManager.Instance.playerHp = GameManager.Instance.maxHp;

            messageText.text +=
                $"\nレベルアップ！" +
                $"\nHP +{hpUp}" +
                $"\n攻撃 +{atkUp}";
        }
    }

    public void RunAway()
    {
        if (battleEnded) return;

        battleEnded = true;
        messageText.text = "逃げ出した！";
        Invoke(nameof(ReturnToDungeon), 1.0f);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.playerHp = playerHp;
        }
    }

    public void Heal()
    {
        if (battleEnded) return;

        if (GameManager.Instance != null && GameManager.Instance.potionCount > 0)
        {
            int healAmount = 10;

            playerHp += healAmount;
            if (playerHp > GameManager.Instance.maxHp)
            {
                playerHp = GameManager.Instance.maxHp;
            }

            GameManager.Instance.potionCount--;

            messageText.text = $"ポーション使用！{healAmount}回復した！";

            // 敵の反撃
            playerHp -= enemyAttack;
            messageText.text += $"\n{currentEnemy.name}の反撃！{enemyAttack}ダメージ！";


            if (playerHp <= 0)
            {
                playerHp = 0;
                battleEnded = true;
                messageText.text += "\nやられてしまった…";
                Invoke(nameof(ReturnToDungeon), 1.5f);
                return;
            }

            RefreshUI();
        }
        else
        {
            messageText.text = "ポーションがない！";
        }
    }

    private void RefreshUI()
    {
        playerHpText.text = $"HP: {playerHp}";
        playerHpText.color = playerHp < 10 ? Color.red : Color.white;
        enemyHpText.text = $"{currentEnemy.name} HP: {enemyHp}";

        if (GameManager.Instance != null)
        {
            levelText.text = $"Lv: {GameManager.Instance.playerLevel}";
            expText.text = $"EXP: {GameManager.Instance.playerExp}/{GameManager.Instance.nextExp}";
            potionText.text = $"Potion: {GameManager.Instance.potionCount}";
        }
    }

    private void ReturnToDungeon()
    {
        SceneManager.LoadScene("Dungeon");
    }


}

