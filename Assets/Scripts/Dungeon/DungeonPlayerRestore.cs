using UnityEngine;

public class DungeonPlayerRestore : MonoBehaviour
{
    private void Start()
    {
        if (GameManager.Instance != null)
        {
            transform.position = GameManager.Instance.dungeonPlayerPosition;
            transform.rotation = GameManager.Instance.dungeonPlayerRotation;
        }
    }
}