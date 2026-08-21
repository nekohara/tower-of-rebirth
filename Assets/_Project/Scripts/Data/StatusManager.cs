using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class StatusManager : MonoBehaviour
{
    [Header("基本情報")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text weightText;

    [Header("能力値")]
    [SerializeField] private TMP_Text strengthText;
    [SerializeField] private TMP_Text vitalityText;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text dexterityText;
    [SerializeField] private TMP_Text intelligenceText;
    [SerializeField] private TMP_Text luckText;

    [Header("装備")]
    [SerializeField] private TMP_Text weaponText;
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text armorText;
    [SerializeField] private TMP_Text defenseText;

    [Header("装備変更")]
    [SerializeField] private TMP_Dropdown weaponDropdown;
    [SerializeField] private TMP_Dropdown armorDropdown;

    private static readonly List<GameObject> hiddenSceneRoots =
    new List<GameObject>();

    private static string sourceSceneName;

    private static bool isOpeningStatus;

    private void Start()
    {
        InitializeEquipmentDropdowns();
        UpdateStatusDisplay();
    }

    private void UpdateStatusDisplay()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.playerStatus == null)
        {
            Debug.LogWarning("プレイヤーステータスを取得できません。");
            return;
        }

        GameManager gm = GameManager.Instance;
        PlayerStatus status = gm.playerStatus;

        nameText.text = $"名前：{status.playerName}";
        hpText.text = $"HP：{status.hp} / {status.maxHp}";
        goldText.text = $"所持金：{status.gold} G";
        weightText.text =
            $"所持重量：{status.GetCurrentWeight():0.0} / {status.GetMaxCarryWeight():0.0}";

        strengthText.text = $"腕力：{status.strength}";
        vitalityText.text = $"体力：{status.vitality}";
        speedText.text = $"素早さ：{status.speed}";
        dexterityText.text = $"器用さ：{status.dexterity}";
        intelligenceText.text = $"知力：{status.intelligence}";
        luckText.text = $"運：{status.luck}";

        weaponText.text = $"武器：{gm.weaponName}";
        attackText.text =
            $"攻撃力：{status.GetAttackPower(gm.weaponPower)}";

        armorText.text = $"防具：{gm.armorName}";
        defenseText.text = $"防御力：{status.GetDefensePower(gm.armorDefense)}";
    }

    public void OnWeaponChanged(int index)
    {
        GameManager gm = GameManager.Instance;

        if (gm == null ||
            index < 0 ||
            index >= gm.ownedWeapons.Count)
        {
            return;
        }

        gm.EquipWeapon(gm.ownedWeapons[index]);
        UpdateStatusDisplay();
    }

    public void OnArmorChanged(int index)
    {
        GameManager gm = GameManager.Instance;

        if (gm == null ||
            index < 0 ||
            index >= gm.ownedArmors.Count)
        {
            return;
        }

        gm.EquipArmor(gm.ownedArmors[index]);
        UpdateStatusDisplay();
    }

    private void InitializeEquipmentDropdowns()
    {
        GameManager gm = GameManager.Instance;

        if (gm == null)
            return;

        weaponDropdown.ClearOptions();

        List<string> weaponOptions = new List<string>();

        foreach (Weapon weapon in gm.ownedWeapons)
            weaponOptions.Add($"{weapon.name}　攻撃力 +{weapon.power}");

        weaponDropdown.AddOptions(weaponOptions);

        int weaponIndex =
            gm.ownedWeapons.FindIndex(weapon => weapon.name == gm.weaponName);

        weaponDropdown.SetValueWithoutNotify(
            weaponIndex >= 0 ? weaponIndex : 0
        );

        armorDropdown.ClearOptions();

        List<string> armorOptions = new List<string>();

        foreach (Armor armor in gm.ownedArmors)
            armorOptions.Add($"{armor.name}　防御力 +{armor.defense}");

        armorDropdown.AddOptions(armorOptions);

        int armorIndex =
            gm.ownedArmors.FindIndex(armor => armor.name == gm.armorName);

        armorDropdown.SetValueWithoutNotify(
            armorIndex >= 0 ? armorIndex : 0
        );
    }

    public static void OpenStatus()
    {
        if (isOpeningStatus)
        {
            return;
        }

        Scene sourceScene = SceneManager.GetActiveScene();

        if (sourceScene.name == "Status")
        {
            return;
        }

        isOpeningStatus = true;
        sourceSceneName = sourceScene.name;
        hiddenSceneRoots.Clear();
        foreach (GameObject root in sourceScene.GetRootGameObjects())
        {
            if (root.activeSelf)
            {
                hiddenSceneRoots.Add(root);

                // Statusを読み込む前に元シーンを無効化
                root.SetActive(false);
            }
        }

        AsyncOperation loadOperation =
            SceneManager.LoadSceneAsync(
                "Status",
                LoadSceneMode.Additive);

        if (loadOperation == null)
        {
            Debug.LogError(
                "Statusシーンのロードを開始できませんでした。");

            foreach (GameObject root in hiddenSceneRoots)
            {
                if (root != null)
                {
                    root.SetActive(true);
                }
            }

            hiddenSceneRoots.Clear();
            isOpeningStatus = false;
            return;
        }

        loadOperation.completed += _ =>
        {
            Scene statusScene =
                SceneManager.GetSceneByName("Status");

            if (!statusScene.IsValid() ||
                 !statusScene.isLoaded)
            {
                Debug.LogError(
                    "Statusシーンのロードに失敗しました。");

                foreach (GameObject root in hiddenSceneRoots)
                {
                    if (root != null)
                    {
                        root.SetActive(true);
                    }
                }

                hiddenSceneRoots.Clear();
                isOpeningStatus = false;
                return;
            }
            SceneManager.SetActiveScene(statusScene);
            isOpeningStatus = false;
        };
    }

    public void OnClickBack()
    {
        Scene statusScene = gameObject.scene;

        Scene sourceScene =
            SceneManager.GetSceneByName(sourceSceneName);

        if (sourceScene.IsValid() &&
            sourceScene.isLoaded)
        {
            SceneManager.SetActiveScene(sourceScene);
        }

        AsyncOperation unloadOperation =
            SceneManager.UnloadSceneAsync(statusScene);

        if (unloadOperation == null)
        {
            Debug.LogError(
                "Statusシーンのアンロードを開始できませんでした。");
            return;
        }

        unloadOperation.completed += _ =>
        {
            foreach (GameObject root in hiddenSceneRoots)
            {
                if (root != null)
                {
                    root.SetActive(true);
                }
            }

            hiddenSceneRoots.Clear();
        };
    }
}