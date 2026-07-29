using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    private bool isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
    }

    public void LoadTitle()
    {
        LoadScene("Title");
    }

    public void LoadPrologue()
    {
        LoadScene("Prologue");
    }


    public void LoadDungeon()
    {
        LoadScene("Dungeon");
    }

    public void LoadBattle()
    {
        LoadScene("Battle");
    }

    public void LoadTown()
    {
        LoadScene("Town");
    }

    public void LoadPlayerCreation()
    {
        LoadScene("PlayerCreation");
    }

    public void LoadScene(string sceneName)
    {
        if (isTransitioning)
            return;

        StartCoroutine(LoadSceneWithFade(sceneName));
    }

    public void FadeTransition(Action action, Action onComplete = null)
    {
        if (isTransitioning)
            return;

        StartCoroutine(LoadActionWithFade(action, onComplete));
    }

    private IEnumerator LoadActionWithFade(
        Action action,
        Action onComplete)
    {
        isTransitioning = true;
        fadeCanvasGroup.blocksRaycasts = true;

        // à√ì]
        yield return Fade(0f, 1f);

        // à√ì]íÜÇ…âÊñ ÇêÿÇËë÷Ç¶ÇÈ
        action?.Invoke();

        // êÿÇËë÷Ç¶ÇΩUIÇîΩâfÇµÇƒÇ©ÇÁñæì]
        yield return null;
        yield return Fade(1f, 0f);

        fadeCanvasGroup.blocksRaycasts = false;
        isTransitioning = false;

        onComplete?.Invoke();
    }

    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        isTransitioning = true;
        fadeCanvasGroup.blocksRaycasts = true;

        // âÊñ Çà√ì]
        yield return Fade(0f, 1f);

        // à√ì]ÇµÇΩÇ‹Ç‹éüÇÃÉVÅ[ÉìÇì«Ç›çûÇﬁ
        AsyncOperation loadOperation =
            SceneManager.LoadSceneAsync(sceneName);

        while (!loadOperation.isDone)
        {
            yield return null;
        }

        // êVÇµÇ¢ÉVÅ[ÉìÇï\é¶
        yield return Fade(1f, 0f);

        fadeCanvasGroup.blocksRaycasts = false;
        isTransitioning = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsedTime = 0f;
        fadeCanvasGroup.alpha = from;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float rate =
                Mathf.Clamp01(elapsedTime / fadeDuration);

            fadeCanvasGroup.alpha =
                Mathf.Lerp(from, to, rate);

            yield return null;
        }

        fadeCanvasGroup.alpha = to;
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Game Exit");
    }
}