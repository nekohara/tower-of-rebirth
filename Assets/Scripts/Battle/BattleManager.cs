using System.Collections.Generic;
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
        Paralysis,
        Sleep,
        Heal
    }

    private enum StatusEffect
    {
        None,
        Poison,
        Paralysis,
        Sleep
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

    [System.Serializable]
    public class Skill
    {
        // powerMultiplier: 攻撃倍率
        // healAmount: 回復量
        // isHealSkill: 回復スキルかどうか
        public string name;
        public int powerMultiplier;
        public int healAmount;
        public bool isHealSkill;
        public int maxUseCount;
        public int currentUseCount;

        public Skill(string name, int powerMultiplier, int healAmount, bool isHealSkill)
        {
            this.name = name;
            this.powerMultiplier = powerMultiplier;
            this.healAmount = healAmount;
            this.isHealSkill = isHealSkill;
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
    [SerializeField] private GameObject commandPanel;
    [SerializeField] private GameObject skillPanel;

    private int playerHp;
    private int playerAttack;

    private int enemyHp;
    private int enemyAttack;

    private bool battleEnded = false;

    private Enemy currentEnemy;

    private int poisonDamage = 2;

    private int enemyHealAmount = 5;

    private int enemyMaxHp;

    private List<StatusEffect> playerStatus = new List<StatusEffect>();

    private List<Skill> skills = new List<Skill>();

    private void Start()
    {

        InitializePlayer();
        SetupEnemy();
        InitializeSkills();
        InitializeBattleUI();
    }

    #region 初期化
    private void SetupEnemy()
    {

        var enemies = new Enemy[]
        {
            new Enemy("スライム", 10, 2, 3, 2, EnemyType.Normal),
            new Enemy("ゴブリン", 15, 3, 5, 4, EnemyType.Poison),
            new Enemy("オオカミ", 12, 4, 4, 3, EnemyType.Double),
            new Enemy("バット", 10, 3, 4, 3, EnemyType.Paralysis),
            new Enemy("スリープゴースト", 8, 2, 6, 5, EnemyType.Sleep),
            new Enemy("ヒーラーゴブリン", 12, 2, 6, 5, EnemyType.Heal)
        };


        currentEnemy = enemies[Random.Range(0, enemies.Length)];


        int level = GameManager.Instance != null ? GameManager.Instance.playerLevel : 1;
        int levelBonus = Mathf.Max(0, level - 1);
        enemyHp = currentEnemy.hp + levelBonus * 2;
        enemyAttack = currentEnemy.attack + levelBonus;
        currentEnemy.exp += levelBonus;

        enemyMaxHp = currentEnemy.hp + levelBonus * 2;
        enemyHp = enemyMaxHp;

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

        if (commandPanel != null) commandPanel.SetActive(true);
        if (skillPanel != null) skillPanel.SetActive(false);
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

        playerStatus.Clear();

        StatusText.text = "";

    }

    private void InitializeSkills()
    {
        Skill powerStrike = new Skill("パワーストライク", 2, 0, false);
        powerStrike.maxUseCount = 3;
        powerStrike.currentUseCount = 3;

        Skill healSkill = new Skill("ヒール", 0, 15, true);
        healSkill.maxUseCount = 2;
        healSkill.currentUseCount = 2;

        skills.Add(powerStrike);
        skills.Add(healSkill); 
    }

    #endregion

    #region コマンド処理
    public void Fight()
    {
        if (battleEnded) return;

        messageText.text = "";

        bool canAct = ApplyStatusEffectsAtTurnStart();

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
        string attackMessage = "";

        if (canAct)
        {
            attackMessage = $"{currentEnemy.name}に{playerAttack}ダメージ！";
        }

        ExecuteEnemyTurn(canAct, attackMessage);


        RefreshUI();

    }

    private string TryApplyStatus()
    {
        if (currentEnemy.type == EnemyType.Poison && Random.value < 0.4f && !playerStatus.Contains(StatusEffect.Poison))
        {
            playerStatus.Add(StatusEffect.Poison);
            return "\n毒を受けた！";
        }

        if (currentEnemy.type == EnemyType.Paralysis && Random.value < 0.3f && !playerStatus.Contains(StatusEffect.Paralysis))
        {
            playerStatus.Add(StatusEffect.Paralysis);
            return "\n体がしびれた！";
        }

        if (currentEnemy.type == EnemyType.Sleep && Random.value < 0.3f && !playerStatus.Contains(StatusEffect.Sleep))
        {
            playerStatus.Add(StatusEffect.Sleep);
            return "\n眠ってしまった！";
        }
        return "";
    }

    private bool ApplyStatusEffectsAtTurnStart()
    {
        bool canAct = true;

        if (playerStatus.Contains(StatusEffect.Sleep))
        {
            if (Random.value < 0.4f)
            {
                playerStatus.Remove(StatusEffect.Sleep);
                messageText.text += "目を覚ました！";
            }
            else
            {
                messageText.text += "眠っていて動けない！";
                canAct = false;
            }
        }

        if (playerStatus.Contains(StatusEffect.Paralysis))
        {
            if (Random.value < 0.5f)
            {
                playerStatus.Remove(StatusEffect.Paralysis);
                if (messageText.text != "") messageText.text += "\n";
                messageText.text += "しびれが治った！";
            }
            else
            {
                if (messageText.text != "") messageText.text += "\n";
                messageText.text += "体がしびれて動けない！";
                canAct = false;
            }
        }

        return canAct;
    }


    private void ApplyStatusEffectsAfterEnemyAction()
    {
        if (playerStatus.Contains(StatusEffect.Poison))
        {
            playerHp -= poisonDamage;
            messageText.text += $"\n毒で{poisonDamage}ダメージ受けた！";
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

            ApplyStatusEffectsAfterEnemyAction();


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

    public void UsePowerStrike()
    {
        UseSkill(skills.Find(s => s.name == "パワーストライク"));
    }

    public void UseHealSkill()
    {
        UseSkill(skills.Find(s => s.name == "ヒール"));
    }

    private void UseSkill(Skill skill)
    {
        if (battleEnded) return;

        if (skill == null)
        {
            return;
        }

        messageText.text = "";

        bool canAct = ApplyStatusEffectsAtTurnStart();

        if (!canAct)
        {
            ExecuteEnemyTurn(false, $"{skill.name}を使えなかった！");
            return;
        }

        if (skill.currentUseCount <= 0)
        {
            messageText.text = $"{skill.name}はもう使えない！";
            return;
        }

        if (skill.isHealSkill)
        {
            playerHp += skill.healAmount;
            if (playerHp > GetTotalMaxHp())
            {
                playerHp = GetTotalMaxHp();
            }

            messageText.text += $"{skill.name}！\nHPが{skill.healAmount}回復した！";
        }
        else
        {
            int skillDamage = playerAttack * skill.powerMultiplier;
            enemyHp -= skillDamage;

            messageText.text += $"{skill.name}！\n{currentEnemy.name}に{skillDamage}ダメージ！";

            if (enemyHp <= 0)
            {
                enemyHp = 0;
                RefreshUI();
                WinBattle();
                return;
            }
        }

        skill.currentUseCount--;

        ExecuteEnemyTurn(true, "");
    }

    #endregion

    #region 敵処理
    private bool TryEnemyHeal()
    {
        if (currentEnemy.type != EnemyType.Heal) return false;

        if (enemyHp < enemyMaxHp * 0.5f)
        {
            int heal = Random.Range(3, (int)(enemyHealAmount * 1.5f) + 1);
            enemyHp += heal;

            if (enemyHp > enemyMaxHp)
            {
                enemyHp = enemyMaxHp;
            }

            messageText.text += $"\n{currentEnemy.name}は回復した！(+{heal})";
            return true;
        }

        return false;
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


    private void ExecuteEnemyTurn(bool acted, string prefixMessage)
    {
        bool didHeal = TryEnemyHeal();

        string msg = "";
        int damage = 0;

        if (!didHeal)
        {
            damage = CalculateEnemyDamage(out msg);
            playerHp -= damage;
        }

        string statusMsg = TryApplyStatus();

        if (prefixMessage != "")
        {
            if (messageText.text != "") messageText.text += "\n";
            messageText.text += prefixMessage;
        }

        if (!didHeal)
        {
            if (acted)
            {
                messageText.text += $"\n{currentEnemy.name}の反撃！";
            }
            else
            {
                messageText.text += $"\n{currentEnemy.name}の攻撃！";
            }

            messageText.text += msg;
            messageText.text += $"\n{damage}ダメージ！";
        }

        messageText.text += statusMsg;

        ApplyStatusEffectsAfterEnemyAction();

        if (playerStatus.Contains(StatusEffect.Sleep) && damage > 0)
        {
            playerStatus.Remove(StatusEffect.Sleep);
            messageText.text += "\n痛みで目を覚ました！";
        }

        if (playerHp <= 0)
        {
            playerHp = 0;
            battleEnded = true;
            RefreshUI();
            messageText.text += "やられてしまった…";
            Invoke(nameof(ReturnToDungeon), 1.5f);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.playerHp = playerHp;
            }
            return;
        }

        RefreshUI();
    }
    #endregion

    #region システム処理

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

        StatusText.text = "";
        foreach (StatusEffect effect in playerStatus)
        {
            switch (effect)
            {
                case StatusEffect.None:
                    StatusText.text = "";
                    break;
                case StatusEffect.Poison:
                    StatusText.text += "毒 ";
                    break;
                case StatusEffect.Paralysis:
                    StatusText.text += "麻痺 ";
                    break;
                case StatusEffect.Sleep:
                    StatusText.text += "睡眠 ";
                    break;
            }
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

    public void OpenSkillPanel()
    {
        if (commandPanel != null) commandPanel.SetActive(false);
        if (skillPanel != null) skillPanel.SetActive(true);
    }

    public void CloseSkillPanel()
    {
        if (skillPanel != null) skillPanel.SetActive(false);
        if (commandPanel != null) commandPanel.SetActive(true);
    }
    #endregion

}

