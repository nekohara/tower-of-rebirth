using UnityEngine;

public class TitleManager : MonoBehaviour
{
    public void OnClickNewGame()
    {
        if (SceneLoader.Instance == null)
        {
            Debug.LogError(
                "SceneLoader.Instance‚ªŒ©‚Â‚©‚è‚Ü‚¹‚ñB");
            return;
        }

        SceneLoader.Instance.LoadPrologue();
    }
}