using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class WazardDialogue : MonoBehaviour
{
    [Header("Referencias del Canvas existente")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private UnityEvent onDialogueFinished;

    [Header("Líneas de Wazard")]
    [SerializeField, TextArea(4, 6)] private string[] dialogueLines;
    [SerializeField] private float typingTime = 0.05f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Transición al Cuestionario")]
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private bool autoStartOnTrigger = false;

    public event Action OnWazardDialogueFinished;

    private bool isPlayerInRange;
    private bool isDialogueActive;
    private bool isFinished;
    private int lineIndex;
    private Coroutine typingCoroutine;

    private void Update()
    {
        if (isFinished || !isDialogueActive)
        {
            if (!isFinished && isPlayerInRange && !autoStartOnTrigger && Input.GetKeyDown(interactKey))
            {
                StartWazardDialogue();
            }
            return;
        }

        if (Input.GetKeyDown(interactKey))
        {
            if (dialogueText.text == dialogueLines[lineIndex])
            {
                NextLine();
            }
            else
            {
                CompleteCurrentLineInstantly();
            }
        }
    }

    public void StartWazardDialogue()
    {
        if (isFinished || isDialogueActive || dialogueLines == null || dialogueLines.Length == 0)
            return;

        isDialogueActive = true;
        lineIndex = 0;
        Time.timeScale = 0f;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        StartTypingLine();
    }

    private void StartTypingLine()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(ShowLine());
    }

    private void CompleteCurrentLineInstantly()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueText.text = dialogueLines[lineIndex];
    }

    private void NextLine()
    {
        lineIndex++;

        if (lineIndex < dialogueLines.Length)
        {
            StartTypingLine();
        }
        else
        {
            FinishDialogueAndOpenQuiz();
        }
    }

    private IEnumerator ShowLine()
    {
        dialogueText.text = string.Empty;

        foreach (char ch in dialogueLines[lineIndex])
        {
            dialogueText.text += ch;
            yield return new WaitForSecondsRealtime(typingTime);
        }

        typingCoroutine = null;
    }

    private void FinishDialogueAndOpenQuiz()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isDialogueActive = false;
        isFinished = true;

        // Cierra el panel de diálogo pero NO restaura Time.timeScale a 1
        // porque el juego continúa pausado durante el cuestionario.
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (quizPanel != null)
            quizPanel.SetActive(true);

        OnWazardDialogueFinished?.Invoke();
        onDialogueFinished?.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        isPlayerInRange = true;

        if (autoStartOnTrigger && !isFinished)
        {
            StartWazardDialogue();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }
}
