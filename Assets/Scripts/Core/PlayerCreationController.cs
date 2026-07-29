using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCreationController : MonoBehaviour
{
    private const int MinimumStat = 1;
    private const int FreeBonusPoints = 6;
    private const int MaximumInitialStat = 6;

    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_Text remainingPointText;
    [SerializeField] private TMP_Text errorText;

    [SerializeField] private TMP_Text strengthText;
    [SerializeField] private TMP_Text vitalityText;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text dexterityText;
    [SerializeField] private TMP_Text intelligenceText;
    [SerializeField] private TMP_Text luckText;

    [SerializeField] private TMP_Dropdown backgroundDropdown;
    [SerializeField] private TMP_Text backgroundDescriptionText;
    [SerializeField] private TMP_Text backgroundBonusText;

    [SerializeField]
    private GameObject guildDialogueController;

    private int baseStrength = MinimumStat;
    private int baseVitality = MinimumStat;
    private int baseSpeed = MinimumStat;
    private int baseDexterity = MinimumStat;
    private int baseIntelligence = MinimumStat;
    private int baseLuck = MinimumStat;

    private int remainingPoints = FreeBonusPoints;

    private string selectedBackgroundId;
    private string selectedBackgroundName;

    private int strength = MinimumStat;
    private int vitality = MinimumStat;
    private int speed = MinimumStat;
    private int dexterity = MinimumStat;
    private int intelligence = MinimumStat;
    private int luck = MinimumStat;




    private void Start()
    {
        if (GameManager.Instance == null)
        {
            GameObject gameManagerObject =
                new GameObject("GameManager");

            gameManagerObject.AddComponent<GameManager>();
        }

        backgroundDropdown.ClearOptions();

        backgroundDropdown.AddOptions(new System.Collections.Generic.List<string>
{
    "都市の用心棒",
    "辺境の狩人",
    "遺跡技師の徒弟",
    "巡礼者"
});

        backgroundDropdown.onValueChanged.AddListener(SelectBackground);

        SelectBackground(0);

        errorText.text = "";
        RefreshUI();
    }

    public void IncreaseStat(int statIndex)
    {
        if (remainingPoints <= 0)
            return;

        if (GetStat(statIndex) >= MaximumInitialStat)
            return;

        ChangeStat(statIndex, 1);
        remainingPoints--;
        RefreshUI();
    }

    public void DecreaseStat(int statIndex)
    {
        if (GetStat(statIndex) <= GetBaseStat(statIndex))
            return;

        ChangeStat(statIndex, -1);
        remainingPoints++;
        RefreshUI();
    }

    private int GetStat(int statIndex)
    {
        return statIndex switch
        {
            0 => strength,
            1 => vitality,
            2 => speed,
            3 => dexterity,
            4 => intelligence,
            5 => luck,
            _ => MinimumStat
        };
    }

    private void ChangeStat(int statIndex, int amount)
    {
        switch (statIndex)
        {
            case 0: strength += amount; break;
            case 1: vitality += amount; break;
            case 2: speed += amount; break;
            case 3: dexterity += amount; break;
            case 4: intelligence += amount; break;
            case 5: luck += amount; break;
        }
    }

    private void RefreshUI()
    {
        remainingPointText.text = $"未配分ポイント：{remainingPoints}";

        strengthText.text = strength.ToString();
        vitalityText.text = vitality.ToString();
        speedText.text = speed.ToString();
        dexterityText.text = dexterity.ToString();
        intelligenceText.text = intelligence.ToString();
        luckText.text = luck.ToString();
    }

    public void RegisterPlayer()
    {
        string playerName = nameInput.text.Trim();

        errorText.text = "";


        if (string.IsNullOrEmpty(playerName))
        {
            errorText.text = "氏名を入力してください";
            return;
        }

        if (remainingPoints > 0)
        {
            errorText.text = "能力ポイントをすべて配分してください";
            return;
        }

        if (GameManager.Instance == null)
        {
            errorText.text = "ゲームデータを初期化できません";
            return;
        }

        PlayerStatus status = new PlayerStatus
        {
            playerName = playerName,
            maxHp = 20,
            hp = 20,
            strength = strength,
            vitality = vitality,
            speed = speed,
            dexterity = dexterity,
            intelligence = intelligence,
            luck = luck,
            backgroundId = selectedBackgroundId,
            backgroundName = selectedBackgroundName,
        };

        GameManager.Instance.playerStatus = status;
        GameManager.Instance.playerExp = 0;
        GameManager.Instance.hasDungeonPosition = false;
        GameManager.Instance.hasDungeonMap = false;



        guildDialogueController.GetComponent<GuildDialogueController>().ShowRegistrationCompleteDialogue();
    }

    private void SelectBackground(int index)
    {
        baseStrength = MinimumStat;
        baseVitality = MinimumStat;
        baseSpeed = MinimumStat;
        baseDexterity = MinimumStat;
        baseIntelligence = MinimumStat;
        baseLuck = MinimumStat;

        switch (index)
        {
            case 0:
                selectedBackgroundId = "guard";
                selectedBackgroundName = "都市の用心棒";

                baseStrength += 2;
                baseVitality += 2;

                backgroundDescriptionText.text =
                    "荒事と護衛で生計を立ててきた。";

                backgroundBonusText.text =
                    "腕力+2、体力+2";
                break;

            case 1:
                selectedBackgroundId = "hunter";
                selectedBackgroundName = "辺境の狩人";

                baseSpeed += 2;
                baseDexterity += 2;

                backgroundDescriptionText.text =
                    "魔物の気配を読み、野山を駆けてきた。";

                backgroundBonusText.text =
                    "素早さ+2、器用さ+2";
                break;

            case 2:
                selectedBackgroundId = "engineer";
                selectedBackgroundName = "遺跡技師の徒弟";

                baseDexterity += 2;
                baseIntelligence += 2;

                backgroundDescriptionText.text =
                    "旧文明の遺物を修復する技術を学んだ。";

                backgroundBonusText.text =
                    "器用さ+2、知力+2";
                break;

            case 3:
                selectedBackgroundId = "pilgrim";
                selectedBackgroundName = "巡礼者";

                baseVitality += 1;
                baseIntelligence += 2;
                baseLuck += 1;

                backgroundDescriptionText.text =
                    "各地に残された遺跡と聖地を巡ってきた。";

                backgroundBonusText.text =
                    "体力+1、知力+2、運+1";
                break;
        }

        // 経歴変更時は自由配分をリセットする
        strength = baseStrength;
        vitality = baseVitality;
        speed = baseSpeed;
        dexterity = baseDexterity;
        intelligence = baseIntelligence;
        luck = baseLuck;

        remainingPoints = FreeBonusPoints;

        errorText.text = "";
        RefreshUI();
    }

    private int GetBaseStat(int statIndex)
    {
        return statIndex switch
        {
            0 => baseStrength,
            1 => baseVitality,
            2 => baseSpeed,
            3 => baseDexterity,
            4 => baseIntelligence,
            5 => baseLuck,
            _ => MinimumStat
        };
    }



}