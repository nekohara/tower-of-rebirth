using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int playerLevel = 1;
    public int playerExp = 0;
    public int nextExp = 10;
    public int playerAttack = 3;
    public int maxHp = 20;
    public int playerHp = 20;
    public int potionCount = 3;

    /// <summary>
    /// ïêäÌ
    /// </summary>
    public string weaponName = "Ç»Çµ";
    public int weaponPower = 0;

    /// <summary>
    /// ñhãÔ
    /// </summary>
    public string armorName = "Ç»Çµ";
    public int armorHpBonus = 0;

    public bool hasDungeonPosition = false;
    public Vector3 dungeonPlayerPosition = new Vector3(0f, 0.5f, 0f);
    public Quaternion dungeonPlayerRotation = Quaternion.identity;

    public int playerGold = 10;

    public int[,] currentDungeonMap;
    public bool hasDungeonMap = false;

    public Vector2Int dungeonPlayerGridPos;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}