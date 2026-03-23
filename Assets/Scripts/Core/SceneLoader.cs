using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadTitle()
    {
        SceneManager.LoadScene("Title");
    }

    public void LoadDungeon()
    {
        SceneManager.LoadScene("Dungeon");
    }

    public void LoadBattle()
    {
        SceneManager.LoadScene("Battle");
    }

    public void LoadTown()
    {
        SceneManager.LoadScene("Town");
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Game Exit");
    }
}