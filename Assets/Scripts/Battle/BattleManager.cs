using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class BattleManager : MonoBehaviour
{
    private class Enemy
    {
        public string name;
        public int hp;
        public int attack;

        public Enemy(string name, int hp, int attack)
        {
            this.name = name;
            this.hp = hp;
            this.attack = attack;
        }
    }

    [SerializeField] private TMP_Text enemyText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text playerHpText;
    [SerializeField] private TMP_Text enemyHpText;

    private int playerHp;
    private int enemyHp = 10;
    private int playerAttack = 3;
    private int enemyAttack = 2;

    private bool battleEnded = false;

    private Enemy currentEnemy;


    private void Start()
    {
        if (GameManager.Instance != null)
        {
            playerHp = GameManager.Instance.playerHp;
        }
        else
        {
            new GameObject("GameManager").AddComponent<GameManager>();
            playerHp = 20; // デバッグ用
        }


        var enemies = new Enemy[]
        {
        new Enemy("スライム", 10, 2),
        new Enemy("ゴブリン", 15, 3),
        new Enemy("オオカミ", 12, 4)
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
            battleEnded = true;
            RefreshUI();
            messageText.text = $"{currentEnemy.name}を倒した！";
            Invoke(nameof(ReturnToDungeon), 1.5f);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.playerHp = playerHp;
            }
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
        messageText.text = $"{currentEnemy.name}に{playerAttack}ダメージ！\n{currentEnemy.name}の反撃！ {enemyAttack}ダメージ受けた！";
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

    private void RefreshUI()
    {
        playerHpText.text = $"HP: {playerHp}";
        enemyHpText.text = $"{currentEnemy.name} HP: {enemyHp}";
    }

    private void ReturnToDungeon()
    {
        SceneManager.LoadScene("Dungeon");
    }


}

