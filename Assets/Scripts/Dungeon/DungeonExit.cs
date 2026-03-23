using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonExit : MonoBehaviour
{
    [SerializeField] private Transform player;

    public void GoToTown()
    {
        if (GameManager.Instance != null && player != null)
        {
            GameManager.Instance.dungeonPlayerPosition = player.position;
            GameManager.Instance.dungeonPlayerRotation = player.rotation;
        }

        SceneManager.LoadScene("Town");
    }
}