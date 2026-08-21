using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonManager : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text expText;
    [SerializeField] private TMP_Text potionText;
    [SerializeField] private TMP_Text weaponText;
    [SerializeField] private TMP_Text armorText;
    [SerializeField] private TMP_Text dungeonNameText;


    private string dungeonName = "Dungeon";
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        RefreshPlayerUI();

    }

    // Update is called once per frame
    void Update()
    {
        RefreshPlayerUI();
    }

    private void RefreshPlayerUI()
    {
        GameManager gm = GameManager.Instance;

        if (gm != null)
        {
            InventoryItem potion =
                gm.playerStatus.inventory.Find(
                    item => item.id == "potion");

            int potionCount = potion?.count ?? 0;

            if (potionText != null)
            {
                potionText.text = $"Potion: {potionCount}";
            }

            if (weaponText != null)
            {
                weaponText.text =
                    $"武器: {gm.weaponName} (+{gm.weaponPower})";
            }

            if (armorText != null)
            {
                armorText.text =
                    $"防具: {gm.armorName} (+{gm.armorDefense})";
            }
        }

        int floor = gm?.currentDungeonFloor ?? 1;

        if (dungeonNameText != null)
        {
            dungeonNameText.text = $"{dungeonName}:{floor}F";
        }
    }

    private int GetTotalMaxHp()
    {
        if (GameManager.Instance == null) return 20;

        return GameManager.Instance.playerStatus.maxHp;
    }


    [SerializeField]
    private GameObject instructionPanel;

    public void ToggleInstructionPanel()
    {
        instructionPanel.SetActive(!instructionPanel.activeSelf);
    }

    public void OnClickStatus()
    {
        StatusManager.OpenStatus();
    }

    public void OnClickReturnToTown()
    {
        GameManager gm = GameManager.Instance;

        if (gm != null)
        {
            // 戦闘復帰用の位置を町への帰還時には破棄する
            gm.hasDungeonPosition = false;
        }

        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadTown();
        }
        else
        {
            SceneManager.LoadScene("Town");
        }
    }
}
