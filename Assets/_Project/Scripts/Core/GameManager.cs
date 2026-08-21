using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int potionCount = 3;

    /// <summary>
    /// 武器
    /// </summary>
    public string weaponName = "なし";
    public int weaponPower = 0;

    /// <summary>
    /// 防具
    /// </summary>
    public string armorName = "なし";
    public int armorDefense = 0;


    public bool hasDungeonPosition = false;
    public Vector3 dungeonPlayerPosition = new Vector3(0f, 0.5f, 0f);
    public Quaternion dungeonPlayerRotation = Quaternion.identity;

    public int[,] currentDungeonMap;
    public bool[,] dungeonExploredTiles;

    public bool hasDungeonMap = false;

    public Vector2Int dungeonPlayerGridPos;

    public PlayerStatus playerStatus = new PlayerStatus();

    public int currentDungeonFloor = 1;

    public List<Armor> ownedArmors = new List<Armor>()
{
    new Armor("なし", 0, 0)
};

    public List<Weapon> ownedWeapons = new List<Weapon>()
{
    new Weapon("なし", 0, 0)
};

    public Vector2Int dungeonStartPosition;

    private void Awake()
    {
        if (playerStatus == null)
            playerStatus = new PlayerStatus();

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            //EquipArmor(ownedArmors[1]);
        }
        else
        {
            Destroy(gameObject);
        }


    }

    public void ResetDungeonProgress()
    {
        currentDungeonFloor = 1;
        currentDungeonMap = null;
        hasDungeonMap = false;
        hasDungeonPosition = false;
        dungeonPlayerGridPos = Vector2Int.zero;
        dungeonStartPosition = Vector2Int.zero;
        dungeonExploredTiles = null;
        ownedWeapons.Clear();
        ownedWeapons.Add(new Weapon("なし", 0, 0));

        ownedArmors.Clear();
        ownedArmors.Add(new Armor("なし", 0, 0));
    }


    public void ResetGameProgress()
    {
        potionCount = 3;

        weaponName = "なし";
        weaponPower = 0;

        armorName = "なし";
        armorDefense = 0;

        playerStatus = new PlayerStatus();

        //EquipArmor(new Armor("レザーアーマー", 50, 2));

        ResetDungeonProgress();
    }


    public void EquipArmor(Armor armor)
    {
        if (armor == null)
            return;

        armorName = armor.name;
        armorDefense = armor.defense;
    }

    public void EquipWeapon(Weapon weapon)
    {
        if (weapon == null)
            return;

        weaponName = weapon.name;
        weaponPower = weapon.power;
    }


}