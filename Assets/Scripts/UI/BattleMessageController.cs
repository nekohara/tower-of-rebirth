using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleMessageController : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;

    [SerializeField] private int maxLinesPerPage = 4;

    private readonly Queue<string> messagePages = new Queue<string>();

    private bool isDisplaying;
    private bool advanceRequested;

    public bool IsDisplaying => isDisplaying;

    private void Update()
    {
        if (!isDisplaying)
            return;

        if (Input.GetMouseButtonDown(0) ||
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            advanceRequested = true;
        }
    }

    public IEnumerator ShowMessage(string message, float displayTime = 0.5f)
    {
        messagePages.Clear();

        foreach (string page in SplitIntoPages(message))
        {
            messagePages.Enqueue(page);
        }

        isDisplaying = true;

        while (messagePages.Count > 0)
        {
            messageText.text = messagePages.Dequeue();
            advanceRequested = false;

            float elapsedTime = 0f;

            while (!advanceRequested && elapsedTime < displayTime)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        isDisplaying = false;
    }

    private IEnumerable<string> SplitIntoPages(string message)
    {
        string[] lines = message.Split('\n');

        for (int i = 0; i < lines.Length; i += maxLinesPerPage)
        {
            int lineCount = Mathf.Min(maxLinesPerPage, lines.Length - i);

            yield return string.Join(
                "\n",
                lines,
                i,
                lineCount
            );
        }
    }
}