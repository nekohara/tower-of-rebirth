using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private enum BattleState
    {
        Start,
        PlayerCommand,
        ExecutingTurn,
        Win,
        Lose
    }

    private enum BattleCommand
    {
        Attack,
        Skill,
        Defend,
        Item,
        Escape
    }

    private enum ActionResult
    {
        Success,        // 行動成功、敵行動へ
        Failed,         // 行動不成立、コマンド選択へ戻る
        BattleEnded     // 逃走成功など、戦闘終了
    }

    //[CreateAssetMenu(menuName = "RPG/Enemy")]
    private class Enemy
    {
        public string name;
        public int hp;
        public int attack;
        public int exp;
        public int gold;
        public int speed;
        public EnemyType type;
        public GameObject battlePrefab;
    

        public Enemy(string name, int hp, int attack, int speed, int exp, int gold, EnemyType type)
        {
            this.name = name;
            this.hp = hp;
            this.attack = attack;
            this.exp = exp;
            this.gold = gold;
            this.type = type;
            this.speed = speed;
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

    private class BattleAction
    {
        public bool isPlayer;
        public BattleCommand command;
        public int speed;

        public Skill Skill;      // スキル使用時
        public InventoryItem Item;        // アイテム使用時


        public BattleAction(bool isPlayer, BattleCommand command, int speed)
        {
            this.isPlayer = isPlayer;
            this.command = command;
            this.speed = speed;
        }
        public BattleAction()
        {

        }
    }

    private class BattleActionResult
    {
        public ActionResult result;
        public string message;

        public BattleActionResult(ActionResult result, string message)
        {
            this.result = result;
            this.message = message;
        }
    }

    [SerializeField] private TMP_Text enemyText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text enemyHpText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text expText;
    [SerializeField] private TMP_Text potionText;
    [SerializeField] private TMP_Text weaponText;
    [SerializeField] private TMP_Text armorText;
    [SerializeField] private TMP_Text StatusText;
    [SerializeField] private GameObject commandPanel;
    [SerializeField] private GameObject skillPanel;
    [SerializeField] private GameObject messageManger;

    private int playerHp;
    private int playerAttack;

    private int enemyHp;
    private int enemyAttack;

    private bool battleEnded = false;

    private Enemy currentEnemy;

    private int poisonDamage = 2;

    private int enemyHealAmount = 5;

    private int enemyMaxHp;

    private BattleState currentState;

    private List<StatusEffect> playerStatus = new List<StatusEffect>();

    private List<Skill> skills = new List<Skill>();

    private bool isDefending;

    private bool reachedCriticalHpThisBattle;

    private List<GrowthResult> growthResults = new List<GrowthResult>();

    [SerializeField] private Transform enemyRoot;
    [SerializeField] private Transform enemySpawnPoint;

    private GameObject currentEnemyObject;

    [SerializeField] private GameObject goblinPrefab;
    [SerializeField] private GameObject slimePrefab;
    [SerializeField] private GameObject batPrefab;
    [SerializeField] private GameObject gohstPrefab;
    [SerializeField] private GameObject mushPrefab;



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
            new Enemy("スライム", 10, 2, 1, 2, 3,  EnemyType.Normal){
                battlePrefab = slimePrefab
            },
            new Enemy("ゴブリン", 15, 3, 2, 5, 4, EnemyType.Poison)
            {
                battlePrefab = goblinPrefab
            },
            new Enemy("キノコ", 12, 4, 3, 4, 5, EnemyType.Double)
            {
                battlePrefab=mushPrefab
            },
            new Enemy("バット", 10, 3, 3, 3, 3, EnemyType.Paralysis)
            {
                battlePrefab = batPrefab
            },
            new Enemy("スリープゴースト", 8, 2, 2, 6, 5, EnemyType.Sleep)
            {
                battlePrefab = gohstPrefab
            },
            //new Enemy("ヒーラーゴブリン", 12, 2, 3, 6, 5, EnemyType.Heal)
        };


        currentEnemy = enemies[Random.Range(0, enemies.Length)];


        int level = GameManager.Instance != null ? GameManager.Instance.playerStatus.level : 1;
        int levelBonus = Mathf.Max(0, level - 1);
        enemyHp = currentEnemy.hp + levelBonus * 2;
        enemyAttack = currentEnemy.attack + levelBonus;
        currentEnemy.exp += levelBonus;

        enemyMaxHp = currentEnemy.hp + levelBonus * 2;
        enemyHp = enemyMaxHp;

        currentEnemyObject = Instantiate(
       currentEnemy.battlePrefab,
       enemySpawnPoint.position,
       enemySpawnPoint.rotation,
       enemyRoot
   );

    }

    private void InitializeBattleUI()
    {

        RefreshUI();
        enemyText.text = $"{currentEnemy.name}が現れた！";
        messageText.text = "どうする？";

        if (GameManager.Instance != null)
        {
            Debug.Log("現在HP: " + GameManager.Instance.playerStatus.hp);
        }

        currentState = BattleState.Start;


        if (commandPanel != null) commandPanel.SetActive(false);
        if (skillPanel != null) skillPanel.SetActive(false);

        StartCoroutine(EnemyFirstRoutine());
    }


    private void InitializePlayer()
    {
        var gm = GameManager.Instance;
        if (gm == null)
        {
            var go = new GameObject("GameManager");
            gm = go.AddComponent<GameManager>();
        }

        playerHp = gm.playerStatus.hp;
        playerAttack = gm.playerStatus.GetAttackPower(gm.weaponPower);

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
        SelectCommand(BattleCommand.Attack);


    }

    public void Heal()
    {
        var potion = GameManager.Instance.playerStatus.inventory
        .FirstOrDefault(x => x.id == "potion" && x.count > 0);

        SelectCommand(BattleCommand.Item, null, potion);
    }

    public void RunAway()
    {
        SelectCommand(BattleCommand.Escape);
    }
    private void SelectSkill(Skill skill)
    {
        SelectCommand(BattleCommand.Skill, skill);
    }


    public void Defened()
    {
        SelectCommand(BattleCommand.Defend);
    }

    private void SelectCommand(
    BattleCommand command,
    Skill skill = null, InventoryItem item = null)
    {
        if (battleEnded) return;
        if (currentState != BattleState.PlayerCommand) return;

        currentState = BattleState.ExecutingTurn;

        int actionSpeed = command == BattleCommand.Defend ? int.MaxValue
                          : GameManager.Instance.playerStatus.GetEffectiveSpeed() + Random.Range(0, 5);

        var playerAction = new BattleAction
        {
            isPlayer = true,
            command = command,
            Skill = skill,
            speed = actionSpeed,
            Item = item

        };

        StartCoroutine(ExecuteTurn(playerAction));
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

    private IEnumerator ExecuteTurn(BattleAction playerAction)
    {
        commandPanel.SetActive(false);

        var actions = new List<BattleAction>
        {
            playerAction,

            new BattleAction(false, BattleCommand.Attack, currentEnemy.speed + Random.Range(0, 5))
        };

        actions.Sort((a, b) => b.speed.CompareTo(a.speed));

        for (int i = 0; i < actions.Count; i++)
        {
            BattleAction action = actions[i];
            if (battleEnded)
                yield break;

            if (action.isPlayer)
            {
                bool canAct = ApplyStatusEffectsAtTurnStart();

                if (!canAct)
                {
                    RefreshUI();

                    yield return new WaitForSeconds(0.5f);
                    continue;
                }

                BattleActionResult actionResult = ExecutePlayerAction(action);

                if (!string.IsNullOrEmpty(actionResult.message))
                {
                    BattleMessageController messageController =
                        messageManger.GetComponent<BattleMessageController>();

                    yield return messageController.ShowMessage(actionResult.message);
                }


                switch (actionResult.result)
                {
                    case ActionResult.Failed:
                        currentState = BattleState.PlayerCommand;

                        if (commandPanel != null)
                        {
                            commandPanel.SetActive(true);
                        }

                        yield break;

                    case ActionResult.BattleEnded:
                        ReturnToDungeon();
                        yield break;

                    case ActionResult.Success:
                        break;
                }
            }
            else
            {
                ExecuteEnemyTurn(false, "");
            }
            RefreshUI();

            if (CheckBattleEnd())
                yield break;

            bool hasNextAction = i < actions.Count - 1;

            if (!action.isPlayer && hasNextAction)
            {
                yield return new WaitForSeconds(0.5f);
            }
        }

        currentState = BattleState.PlayerCommand;
        commandPanel.SetActive(true);

        messageText.text += "\nどうする？";

        isDefending = false;
    }

    private BattleActionResult ExecutePlayerAction(BattleAction action)
    {
        switch (action.command)
        {
            case BattleCommand.Attack:
                return new BattleActionResult(
                    ActionResult.Success,
                    ExecuteAttack()
                );

            case BattleCommand.Skill:
                return ExecuteSkill(action.Skill);

            case BattleCommand.Item:
                return ExecuteItem(action.Item);

            case BattleCommand.Defend:
                return ExecuteDefend();

            case BattleCommand.Escape:
                return ExecuteEscape();
        }

        return new BattleActionResult(ActionResult.Failed, "");
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

    private void ApplyVictoryGrowth()
    {

        // 戦闘勝利
        AddGrowth(StatusType.MaxHp, 1);

        // 瀕死を経験して勝利
        if (reachedCriticalHpThisBattle)
        {
            AddGrowth(StatusType.MaxHp, 2);
        }
    }

    private bool CheckBattleEnd()
    {
        if (enemyHp <= 0)
        {
            enemyHp = 0;
            RefreshUI();
            WinBattle();
            return true;
        }

        if (playerHp <= 0)
        {
            playerHp = 0;
            battleEnded = true;
            RefreshUI();
            EndBattle(false);
            return true;
        }

        return false;
    }

    private void CheckHpGrowthConditions(int damage)
    {

        // 一定以上のダメージを受けた
        if (damage >= GetTotalMaxHp() * 0.2f)
        {
            AddGrowth(StatusType.MaxHp, 1);
        }

        // この戦闘で初めて瀕死になった
        if (!reachedCriticalHpThisBattle &&
            playerHp > 0 &&
            playerHp <= GetTotalMaxHp() * 0.25f)
        {
            reachedCriticalHpThisBattle = true;
            AddGrowth(StatusType.MaxHp, 3);
        }
    }

    private void ApplyStatusEffectsAfterEnemyAction()
    {
        if (playerStatus.Contains(StatusEffect.Poison))
        {
            playerHp -= poisonDamage;
            messageText.text += $"\n毒で{poisonDamage}ダメージ受けた！";
        }
    }

    private string ExecuteAttack()
    {
        enemyHp -= playerAttack;

        AddGrowth(
            StatusType.Strength,
            1
        );

        return $"{currentEnemy.name}に{playerAttack}ダメージ！";
    }


    private BattleActionResult ExecuteSkill(Skill skill)
    {
        if (skill == null)
        {
            return new BattleActionResult(
                ActionResult.Failed,
                "スキルが選択されていません！"
            );
        }

        if (skill.currentUseCount <= 0)
        {
            return new BattleActionResult(
                ActionResult.Failed,
                $"{skill.name}はもう使えない！"
            );
        }

        skill.currentUseCount--;

        if (skill.isHealSkill)
        {
            int beforeHp = playerHp;

            playerHp = Mathf.Min(
                playerHp + skill.healAmount,
                GetTotalMaxHp()
            );

            int actualHealAmount = playerHp - beforeHp;

            return new BattleActionResult(
                ActionResult.Success,
                $"{skill.name}！\nHPが{actualHealAmount}回復した！"
            );
        }

        int skillDamage = playerAttack * skill.powerMultiplier;
        enemyHp -= skillDamage;

        return new BattleActionResult(
            ActionResult.Success,
            $"{skill.name}！\n" +
            $"{currentEnemy.name}に{skillDamage}ダメージ！"
        );
    }

    private BattleActionResult ExecuteItem(InventoryItem item)
    {
        if (item == null || item.count <= 0)
        {
            return new BattleActionResult(
                ActionResult.Failed,
                "アイテムを持っていない！"
            );
        }

        if (!item.isHealItem)
        {
            return new BattleActionResult(
                ActionResult.Failed,
                "このアイテムは使用できない！"
            );
        }

        if (playerHp >= GetTotalMaxHp())
        {
            return new BattleActionResult(
                ActionResult.Failed,
                "HPは満タンだ！"
            );
        }

        int beforeHp = playerHp;

        playerHp = Mathf.Min(
            playerHp + item.healAmount,
            GetTotalMaxHp()
        );

        int actualHealAmount = playerHp - beforeHp;

        item.count--;

        return new BattleActionResult(
            ActionResult.Success,
            $"{item.name}を使用した！\n" +
            $"HPが{actualHealAmount}回復した！"
        );
    }

    private BattleActionResult ExecuteDefend()
    {
        isDefending = true;


        return new BattleActionResult(
            ActionResult.Success,
             "身を守っている！"
        );
    }

    private BattleActionResult ExecuteEscape()
    {
        int speed = GameManager.Instance.playerStatus.GetEffectiveSpeed();
        int enemySpeed = currentEnemy.speed;

        float escapeRate =
            0.5f +
            (speed - enemySpeed) * 0.08f;

        escapeRate = Mathf.Clamp(escapeRate, 0.1f, 0.9f);

        if (Random.value < escapeRate)
        {

            AddGrowth(
                StatusType.Speed,
                2
            );

            battleEnded = true;

            GameManager.Instance.playerStatus.hp = playerHp;

            return new BattleActionResult(
             ActionResult.BattleEnded,
              "逃げ出した！"
            );
        }


        return new BattleActionResult(
             ActionResult.Success,
              "逃げられなかった！"
            );
    }
    //public void RunAway()
    //{
    //    if (battleEnded) return;

    //    int speed = GameManager.Instance.playerStatus.speed;
    //    int enemySpeed = currentEnemy.speed;

    //    float escapeRate =
    //        0.5f +
    //        (speed - enemySpeed) * 0.08f;

    //    escapeRate = Mathf.Clamp(escapeRate, 0.1f, 0.9f);

    //    if (Random.value < escapeRate)
    //    {

    //        battleEnded = true;
    //        messageText.text = "逃げ出した！";
    //        Invoke(nameof(ReturnToDungeon), 1.0f);
    //        if (GameManager.Instance != null)
    //        {
    //            GameManager.Instance.playerStatus.hp = playerHp;
    //        }

    //    }
    //    else
    //    {
    //        messageText.text = "逃げられなかった！";
    //        ExecuteEnemyTurn(false, "");
    //    }
    //}

    //public void Heal()
    //{
    //    if (battleEnded) return;

    //    if (GameManager.Instance != null && GameManager.Instance.potionCount > 0)
    //    {
    //        int healAmount = 10;

    //        playerHp += healAmount;
    //        if (playerHp > GetTotalMaxHp())
    //        {
    //            playerHp = GetTotalMaxHp();
    //        }

    //        GameManager.Instance.potionCount--;

    //        messageText.text = $"ポーション使用！{healAmount}回復した！";

    //        messageText.text += $"\n{currentEnemy.name}の反撃！";


    //        string msg = "";

    //        int damage = CalculateEnemyDamage(out msg);


    //        // 敵の反撃
    //        playerHp -= damage;

    //        messageText.text += msg;

    //        messageText.text += $"\n{damage}ダメージ！";


    //        string poisonMsg = TryApplyStatus();
    //        messageText.text += poisonMsg;

    //        ApplyStatusEffectsAfterEnemyAction();


    //        if (GameManager.Instance != null)
    //        {
    //            GameManager.Instance.playerStatus.hp = playerHp;
    //        }

    //        if (playerHp <= 0)
    //        {
    //            playerHp = 0;
    //            battleEnded = true;
    //            messageText.text += "\nやられてしまった…";
    //            EndBattle(false);
    //            return;
    //        }

    //        RefreshUI();
    //    }
    //    else
    //    {
    //        messageText.text = "ポーションがない！";
    //    }
    //}

    public void UsePowerStrike()
    {
        SelectSkill(skills[0]);
        if (skillPanel != null)
        {
            skillPanel.SetActive(false);
        }
    }

    public void UseHealSkill()
    {
        SelectSkill(skills[1]);
        if (skillPanel != null)
        {
            skillPanel.SetActive(false);
        }
    }

    private void AddGrowth(StatusType type, int amount)
    {
        GrowthResult result =
            GameManager.Instance.playerStatus.AddGrowth(type, amount);

        if (result.grew)
        {
            growthResults.Add(result);
        }
    }

    #endregion

    #region 敵処理


    private IEnumerator EnemyFirstRoutine()
    {
        bool playerFirst = CheckPlayerFirst();

        if (playerFirst)
        {
            messageText.text = "先制攻撃のチャンス！\nどうする？";
            currentState = BattleState.PlayerCommand;

            if (commandPanel != null) commandPanel.SetActive(true);

            yield break;
        }

        if (commandPanel != null) commandPanel.SetActive(false);

        if (currentEnemy.speed >= 10)
        {
            messageText.text = $"{currentEnemy.name}が電光石火で襲いかかってきた！";
        }
        else if (currentEnemy.speed >= 5)
        {
            messageText.text = $"{currentEnemy.name}が素早く動いた！";
        }
        else
        {
            messageText.text = $"{currentEnemy.name}が動き出した！";
        }
        yield return new WaitForSeconds(1.0f);

        ExecuteEnemyTurn(false, "");

        if (!battleEnded && commandPanel != null)
        {
            currentState = BattleState.PlayerCommand;
            commandPanel.SetActive(true);
        }
    }


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

            if (isDefending)
            {
                damage = Mathf.Max(1, damage / 2);
                AddGrowth(
                    StatusType.Vitality,
                    2
                );

            }
            else
            {
                AddGrowth(
                    StatusType.Vitality,
                    1
                );
            }


                playerHp -= damage;
            CheckHpGrowthConditions(damage);
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
            //Invoke(nameof(ReturnToDungeon), 1.5f);

            EndBattle(false);

            //if (GameManager.Instance != null)
            //{
            //    GameManager.Instance.playerHp = playerHp;
            //}
            return;
        }

        RefreshUI();
    }
    #endregion

    #region システム処理
    private bool CheckPlayerFirst()
    {
        int playerSpeed = GameManager.Instance.playerStatus.GetEffectiveSpeed();
        int enemySpeed = currentEnemy.speed;

        int playerRoll = playerSpeed + Random.Range(0, 5);
        int enemyRoll = enemySpeed + Random.Range(0, 5);

        Debug.Log($"PlayerSpeed:{playerRoll} EnemySpeed:{enemyRoll}");

        return playerRoll >= enemyRoll;
    }

    private string GetGrowthResultMessage()
    {
        if (growthResults.Count == 0)
        {
            return "";
        }

        string message = "\n\n能力が成長した！";

        var groupedResults = growthResults
            .GroupBy(result => result.type)
            .Select(group => new
            {
                Type = group.Key,
                Amount = group.Sum(result => result.amount)
            });

        foreach (var result in groupedResults)
        {
            string statusName = result.Type switch
            {
                StatusType.MaxHp => "最大HP",
                StatusType.Strength => "筋力",
                StatusType.Vitality => "体力",
                StatusType.Speed => "素早さ",
                StatusType.Dexterity => "器用さ",
                StatusType.Intelligence => "知力",
                StatusType.Luck => "運",
                _ => result.Type.ToString()
            };

            message += $"\n{statusName}が{result.Amount}上がった！";
        }

        return message;
    }

    private void WinBattle()
    {
        battleEnded = true;

        StartCoroutine(WinBattleRoutine());
    }

    private IEnumerator WinBattleRoutine()
    {
        string message = "";

        ApplyVictoryGrowth();

        message =
            $"{currentEnemy.name}を倒した！\n" +
            $"{currentEnemy.gold}Gを手に入れた！";

        if (GameManager.Instance != null)
        {
            GameManager.Instance.playerStatus.hp = playerHp;
            GameManager.Instance.playerExp += currentEnemy.exp;
            GameManager.Instance.playerGold += currentEnemy.gold;

        }

        message += GetGrowthResultMessage();

        BattleMessageController messageController =
            messageManger.GetComponent<BattleMessageController>();

        yield return messageController.ShowMessage(message, 3.0f);

        ReturnToDungeon();
    }


    private void RefreshUI()
    {
        enemyHpText.text = $"{currentEnemy.name} HP: {enemyHp}";

        if (GameManager.Instance != null)
        {
            levelText.text = $"Lv: {GameManager.Instance.playerStatus.level}";
            expText.text = $"EXP: {GameManager.Instance.playerExp}/{GameManager.Instance.nextExp}";
            
            weaponText.text = $"武器: {GameManager.Instance.weaponName} (+{GameManager.Instance.weaponPower})";
            armorText.text = $"防具: {GameManager.Instance.armorName} (+HP {GameManager.Instance.armorHpBonus})";

            var potion = GameManager.Instance.playerStatus.inventory .FirstOrDefault(x => x.id == "potion");

            int potionCount = potion?.count ?? 0;

            potionText.text = $"Potion: {potionCount}";


            GameManager.Instance.playerStatus.hp = playerHp;
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

        return GameManager.Instance.playerStatus.maxHp + GameManager.Instance.armorHpBonus;
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

    public void EndBattle(bool isWin)
    {
        if (isWin)
        {
            Invoke(nameof(ReturnToDungeon), 4.0f);
        }
        else
        {
            Invoke(nameof(HandleDefeat), 2.5f);
        }
    }


    private void HandleDefeat()
    {
        GameManager.Instance.playerStatus.hp = Mathf.Max(1, GameManager.Instance.playerStatus.maxHp / 2);

        // 入口座標に戻す場合
        GameManager.Instance.hasDungeonPosition = false;
        GameManager.Instance.dungeonPlayerPosition = Vector3.zero;
        GameManager.Instance.dungeonPlayerRotation = Quaternion.identity;

        // ここ追加
        messageText.text = "意識を失った…\n気がつくと入口にいた…";

        Invoke(nameof(ReturnToDungeon), 2.5f);
    }
    #endregion

}

