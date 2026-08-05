using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndController : MonoBehaviour
{
    public void OnClickReturnToTitle()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGameProgress();
        }

        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadTitle();
        }
        else
        {
            SceneManager.LoadScene("Title");
        }
    }

}