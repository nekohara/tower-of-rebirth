using System;
using TMPro;
using UnityEngine;

public class GuildDialogueController : MonoBehaviour
{
    [Serializable]
    private class DialogueLine
    {
        public string speakerName;

        [TextArea(2, 5)]
        public string message;
    }

    [Header("UI")]
    [SerializeField] private GameObject guildDialoguePanel;
    [SerializeField] private GameObject resumePanel;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text messageText;

    [Header("Dialogue")]
    [SerializeField] private DialogueLine[] dialogueLines;

    [Header("Registration Complete Dialogue")]
    [SerializeField] private DialogueLine[] registrationCompleteLines;

    private bool isRegistrationComplete;

    private int currentLine;
    private bool canAdvance;

    private void Start()
    {
        guildDialoguePanel.SetActive(true);
        resumePanel.SetActive(false);

        currentLine = 0;
        ShowCurrentLine();

        // シーン開始時のクリックで会話が進むのを防止
        Invoke(nameof(EnableAdvance), 0.2f);
    }

    private void Update()
    {
        if (!canAdvance)
            return;

        bool clicked = Input.GetMouseButtonDown(0);
        bool submitted = Input.GetKeyDown(KeyCode.Return);
        bool pressedSpace = Input.GetKeyDown(KeyCode.Space);

        if (clicked || submitted || pressedSpace)
        {
            ShowNextLine();
        }
    }

    private void EnableAdvance()
    {
        canAdvance = true;
    }

    private void ShowCurrentLine()
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            OpenRegistrationForm();
            return;
        }

        DialogueLine line = dialogueLines[currentLine];

        speakerNameText.text = line.speakerName;
        messageText.text = line.message;
    }

    private void ShowNextLine()
    {
        currentLine++;

        if (currentLine >= dialogueLines.Length)
        {
            if (isRegistrationComplete)
            {
                SceneLoader.Instance.LoadTown();
            }
            else
            {
                OpenRegistrationForm();
            }

            return;
        }

        ShowCurrentLine();
    }

    private void OpenRegistrationForm()
    {
        canAdvance = false;
        SceneLoader.Instance.FadeTransition(() =>
        {
            guildDialoguePanel.SetActive(false);
            resumePanel.SetActive(true);
        });
    }

    public void ShowRegistrationCompleteDialogue()
    {
        canAdvance = false;

        SceneLoader.Instance.FadeTransition(
            () =>
            {
                isRegistrationComplete = true;
                dialogueLines = registrationCompleteLines;
                currentLine = 0;

                resumePanel.SetActive(false);
                guildDialoguePanel.SetActive(true);

                ShowCurrentLine();
            },
            () =>
            {
                canAdvance = true;
            });
    }
}