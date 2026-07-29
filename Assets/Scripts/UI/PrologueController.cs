using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PrologueController : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text pageText;
    [SerializeField] private string nextSceneName = "PlayerCreation";

    [TextArea(3, 8)]
    [SerializeField] private string[] pages;

    [SerializeField] private float fadeDuration = 0.3f;

    private bool isFading;

    private int currentPage;
    private bool canAdvance;

    private void Start()
    {
        if (pages == null || pages.Length == 0)
        {
            FinishPrologue();
            return;
        }

        ShowCurrentPage();

        // 開始時のクリックが、そのまま次ページ送りになるのを防止
        Invoke(nameof(EnableAdvance), 0.2f);
    }

    private void Update()
    {
        if (!canAdvance || isFading)
            return;

        if (Input.GetMouseButtonDown(0) ||
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            ShowNextPage();
        }
    }

    private void EnableAdvance()
    {
        canAdvance = true;
    }

    private void ShowNextPage()
    {
        if (isFading)
            return;

        if (currentPage + 1 >= pages.Length)
        {
            FinishPrologue();
            return;
        }

        StartCoroutine(ChangePageWithFade());
    }

    private void ShowCurrentPage()
    {
        messageText.text = pages[currentPage];

        if (pageText != null)
        {
            pageText.text = $"{currentPage + 1} / {pages.Length}";
        }
    }

    private IEnumerator ChangePageWithFade()
    {
        isFading = true;

        // フェードアウト
        yield return FadeMessage(1f, 0f);

        currentPage++;
        ShowCurrentPage();

        // フェードイン
        yield return FadeMessage(0f, 1f);

        isFading = false;
    }

    private IEnumerator FadeMessage(float from, float to)
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            float rate = Mathf.Clamp01(elapsedTime / fadeDuration);
            messageText.alpha = Mathf.Lerp(from, to, rate);

            yield return null;
        }

        messageText.alpha = to;
    }

    public void SkipPrologue()
    {
        FinishPrologue();
    }

    private void FinishPrologue()
    {
        SceneLoader.Instance.LoadScene(nextSceneName);
    }
}