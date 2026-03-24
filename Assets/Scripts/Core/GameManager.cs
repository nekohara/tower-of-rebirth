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

    public Vector3 dungeonPlayerPosition = new Vector3(0f, 0.5f, 0f);
    public Quaternion dungeonPlayerRotation = Quaternion.identity;

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