using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    private enum EnemyType
    {
        Normal,
        Double,
        Poison,
        Paralysis
    }

    private enum StatusEffect
    {
        None,
        Poison,
        Paralysis
    }

    private class Enemy
    {
        public string name;
        public int hp;
        public int attack;
        public int exp;
        public int gold;
        public EnemyType type;

        public Enemy(string name, int hp, int attack, int exp, int gold, EnemyType type)
        {
            this.name = name;
            this.hp = hp;
            this.attack = attack;
            this.exp = exp;
            this.gold = gold;
            this.type = type;
        }
    }

    [SerializeField] private TMP_Text enemyText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text playerHpText;
    [SerializeField] private TMP_Text enemyHpText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text expText;
    [SerializeField] private TMP_Text potionText;
    [SerializeField] private TMP_Text weaponText;
    [SerializeField] private TMP_Text armorText;
    [SerializeField] private TMP_Text StatusText;

    private int playerHp;
    private int playerAttack;

    private int enemyHp;
    private int enemyAttack;

    private bool battleEnded = false;

    private Enemy currentEnemy;

    private int poisonDamage = 2;

    private StatusEffect playerStatus = StatusEffect.None;

    private void Start()
    {

        InitializePlayer();
        SetupEnemy();
        InitializeBattleUI();
    }

    private void SetupEnemy()
    {

        var enemies = new Enemy[]
        {
            new Enemy("スライム", 10, 2, 3, 2, EnemyType.Normal),
            new Enemy("ゴブリン", 15, 3, 5, 4, EnemyType.Poison),
            new Enemy("オオカミ", 12, 4, 4, 3, EnemyType.Double),
            new Enemy("バット", 10, 3, 4, 3, EnemyType.Paralysis)
        };


        currentEnemy = enemies[Random.Range(0, enemies.Length)];


        int level = GameManager.Instance != null ? GameManager.Instance.playerLevel : 1;
        int levelBonus = Mathf.Max(0, level - 1);
        enemyHp = currentEnemy.hp + levelBonus * 2;
        enemyAttack = currentEnemy.attack + levelBonus;
        currentEnemy.exp += levelBonus;

    }

    private void InitializeBattleUI()
    {

        RefreshUI();
        enemyText.text = $"{currentEnemy.name}が現れた！";
        messageText.text = "どうする？";

        if (GameManager.Instance != null)
        {
            Debug.Log("現在HP: " + GameManager.Instance.playerHp);
        }
    }


    private void InitializePlayer()
    {
        var gm = GameManager.Instance;
        if (gm == null)
        {
            var go = new GameObject("GameManager");
            gm = go.AddComponent<GameManager>();
        }

        playerHp = gm.playerHp;
        playerAttack = gm.playerAttack + gm.weaponPower;

        playerStatus = StatusEffect.None;

        StatusText.text = "";

    }

    public void Fight()
    {
        if (battleEnded) return;

        messageText.text = "";

        bool canAct = true;

        if (playerStatus == StatusEffect.Paralysis)
        {
            if (Random.value < 0.5f)
            {
                playerStatus = StatusEffect.None;
                messageText.text = "しびれが治った！";
            }
            else
            {
                messageText.text = "体がしびれて動けない！";
                canAct = false;
            }
        }

        if (canAct)
        {
            enemyHp -= playerAttack;

            if (enemyHp <= 0)
            {
                enemyHp = 0;
                RefreshUI();
                WinBattle();
                return;
            }
        }

        string msg = "";

        int damage = CalculateEnemyDamage(out msg);

        playerHp -= damage;

        string poisonMsg = TryApplyStatus();

        if (canAct)
        {
            messageText.text += $"{currentEnemy.name}に{playerAttack}ダメージ！";

            messageText.text += $"\n{currentEnemy.name}の反撃！";
        }else
        {
            messageText.text += $"\n{currentEnemy.name}の攻撃！";
        }

        messageText.text += msg;
        messageText.text += $"\n{damage}ダメージ！";
        messageText.text += poisonMsg;


        messageText.text += ApplyPoisonDamage();


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

    }

    private string TryApplyStatus()
    {
        if (currentEnemy.type == EnemyType.Poison && Random.value < 0.4f)
        {
            playerStatus = StatusEffect.Poison;
            return "\n毒を受けた！";
        }

        if (currentEnemy.type == EnemyType.Paralysis && Random.value < 0.3f)
        {
            playerStatus = StatusEffect.Paralysis;
            return "\n体がしびれた！";
        }

        return "";
    }

    //private string TryApplyPoison()
    //{
    //    if (currentEnemy.type == EnemyType.Poison && Random.value < 0.4f)
    //    {
    //        playerStatus = StatusEffect.Poison;
    //        return "\n毒を受けた！";
    //    }

    //    return "";
    //}

    private string ApplyPoisonDamage()
    {
        if (playerStatus != StatusEffect.Poison) return "";

        playerHp -= poisonDamage;
        return $"\n毒で{poisonDamage}ダメージ受けた！";
    }

    private int CalculateEnemyDamage(out string actionMessage)
    {
        int damage = enemyAttack;
        actionMessage = "";

        if (currentEnemy.type == EnemyType.Double)
        {
            damage += enemyAttack;
            actionMessage += "\n2回攻撃！";
        }

        if (Random.value < 0.3f)
        {
            damage *= 2;
            actionMessage += "\n強攻撃！";
        }

        return damage;
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
            if (playerHp > GetTotalMaxHp())
            {
                playerHp = GetTotalMaxHp();
            }

            GameManager.Instance.potionCount--;

            messageText.text = $"ポーション使用！{healAmount}回復した！";

            messageText.text += $"\n{currentEnemy.name}の反撃！";


            string msg = "";

            int damage = CalculateEnemyDamage(out msg);


            // 敵の反撃
            playerHp -= damage;

            messageText.text += msg;

            messageText.text += $"\n{damage}ダメージ！";


            string poisonMsg = TryApplyStatus();
            messageText.text += poisonMsg;

            messageText.text += ApplyPoisonDamage();


            if (GameManager.Instance != null)
            {
                GameManager.Instance.playerHp = playerHp;
            }

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
        playerHpText.text = $"HP: {playerHp}/{GetTotalMaxHp()}";
        playerHpText.color = playerHp <= GetTotalMaxHp() * 0.3f ? Color.red : Color.white;
        enemyHpText.text = $"{currentEnemy.name} HP: {enemyHp}";

        if (GameManager.Instance != null)
        {
            levelText.text = $"Lv: {GameManager.Instance.playerLevel}";
            expText.text = $"EXP: {GameManager.Instance.playerExp}/{GameManager.Instance.nextExp}";
            potionText.text = $"Potion: {GameManager.Instance.potionCount}";
            weaponText.text = $"武器: {GameManager.Instance.weaponName} (+{GameManager.Instance.weaponPower})";
            armorText.text = $"防具: {GameManager.Instance.armorName} (+HP {GameManager.Instance.armorHpBonus})";
        }

        switch (playerStatus)
        {
            case StatusEffect.None:
                StatusText.text = "";
                break;
            case StatusEffect.Poison:
                StatusText.text = "毒";
                break;
            case StatusEffect.Paralysis:
                StatusText.text = "麻痺";
                break;
        }
    }

    private int GetTotalMaxHp()
    {
        if (GameManager.Instance == null) return 20;

        return GameManager.Instance.maxHp + GameManager.Instance.armorHpBonus;
    }

    private void ReturnToDungeon()
    {
        SceneManager.LoadScene("Dungeon");
    }


}

