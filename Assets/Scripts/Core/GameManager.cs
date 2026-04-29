using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int playerExp = 0;
    public int nextExp = 10;
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

    public PlayerStatus playerStatus = new PlayerStatus();

    private void Awake()
    {
        if (playerStatus == null)
            playerStatus = new PlayerStatus();

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