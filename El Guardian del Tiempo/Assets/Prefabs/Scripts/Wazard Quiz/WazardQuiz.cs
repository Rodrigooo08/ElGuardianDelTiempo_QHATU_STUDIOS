using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WazardQuiz : MonoBehaviour
{
    [Serializable]
    public struct QuizQuestion
    {
        [TextArea(2, 4)]
        public string questionText;
        public string[] options; // Esperado: 3 opciones
        public int correctOptionIndex; // 0, 1 o 2
    }

    [Header("Referencias del Panel")]
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text counterText; // Ej: "(1 / 3)"
    [SerializeField] private Button[] optionButtons; // 3 botones: 0, 1, 2
    [SerializeField] private TMP_Text[] optionTexts; // Textos de los 3 botones
    [SerializeField] private Button nextButton; // Botón "Siguiente"

    [Header("Colores de Estado de Botones")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color correctColor = new Color(0.35f, 0.8f, 0.45f);
    [SerializeField] private Color wrongColor = new Color(0.85f, 0.35f, 0.35f);

    [Header("Controles y Tiempos")]
    [SerializeField] private bool allowNumberKeys = true; // Teclas 1, 2, 3
    [SerializeField] private KeyCode nextKey = KeyCode.Return;
    [SerializeField] private float inputLockDelay = 0.25f; // Evita clics accidentales al abrir

    [Header("Preguntas del Nivel")]
    [SerializeField] private QuizQuestion[] questions;

    public event Action<int, int> OnQuizCompleted; // (aciertos, totalPreguntas)

    private int currentQuestionIndex;
    private int score;
    private bool hasAnsweredCurrent;
    private bool canAcceptInput;

    private void Awake()
    {
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextButtonClicked);
            nextButton.interactable = false;
        }

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i; // Captura local segura para el listener
            if (optionButtons[i] != null)
            {
                optionButtons[i].onClick.AddListener(() => SelectOption(index));
            }
        }
    }

       /* private void OnEnable()
        {
            StartQuiz();
        }*/

    private void Update()
    {
        if (quizPanel == null || !quizPanel.activeSelf || !canAcceptInput)
            return;

        if (!hasAnsweredCurrent && allowNumberKeys)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
                SelectOption(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                SelectOption(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
                SelectOption(2);
        }
        else if (hasAnsweredCurrent && Input.GetKeyDown(nextKey))
        {
            OnNextButtonClicked();
        }
    }

    public void StartQuiz()
    {
        if (questions == null || questions.Length == 0)
            return;

        Time.timeScale = 0f;
        currentQuestionIndex = 0;
        score = 0;

        if (quizPanel != null)
            quizPanel.SetActive(true);

        LoadQuestion(currentQuestionIndex);
    }

    private void LoadQuestion(int index)
    {
        hasAnsweredCurrent = false;
        canAcceptInput = false;

        QuizQuestion q = questions[index];

        if (questionText != null)
            questionText.text = q.questionText;

        if (counterText != null)
            counterText.text = $"({index + 1} / {questions.Length})";

        for (int i = 0; i < optionButtons.Length; i++)
        {
            bool hasOption = q.options != null && i < q.options.Length;
            if (optionButtons[i] != null)
            {
                optionButtons[i].gameObject.SetActive(hasOption);
                optionButtons[i].interactable = hasOption;
                SetButtonColor(optionButtons[i], normalColor);
            }

            if (hasOption && i < optionTexts.Length && optionTexts[i] != null)
            {
                optionTexts[i].text = q.options[i];
            }
        }

        if (nextButton != null)
            nextButton.interactable = false;

        StartCoroutine(EnableInputAfterDelay());
    }

    private IEnumerator EnableInputAfterDelay()
    {
        yield return new WaitForSecondsRealtime(inputLockDelay);
        canAcceptInput = true;
    }

    public void SelectOption(int selectedIndex)
    {
        if (hasAnsweredCurrent || !canAcceptInput)
            return;

        hasAnsweredCurrent = true;
        QuizQuestion q = questions[currentQuestionIndex];

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (optionButtons[i] != null)
                optionButtons[i].interactable = false;
        }

        if (selectedIndex == q.correctOptionIndex)
        {
            score++;
            if (selectedIndex >= 0 && selectedIndex < optionButtons.Length)
                SetButtonColor(optionButtons[selectedIndex], correctColor);
        }
        else
        {
            if (selectedIndex >= 0 && selectedIndex < optionButtons.Length)
                SetButtonColor(optionButtons[selectedIndex], wrongColor);

            if (q.correctOptionIndex >= 0 && q.correctOptionIndex < optionButtons.Length)
                SetButtonColor(optionButtons[q.correctOptionIndex], correctColor);
        }

        if (nextButton != null)
            nextButton.interactable = true;
    }

    private void OnNextButtonClicked()
    {
        if (!hasAnsweredCurrent || !canAcceptInput)
            return;

        currentQuestionIndex++;

        if (currentQuestionIndex < questions.Length)
        {
            LoadQuestion(currentQuestionIndex);
        }
        else
        {
            FinishQuiz();
        }
    }

    private void FinishQuiz()
    {
        canAcceptInput = false;

        if (quizPanel != null)
            quizPanel.SetActive(false);

        Time.timeScale = 1f;
        OnQuizCompleted?.Invoke(score, questions.Length);
    }

    private void SetButtonColor(Button button, Color color)
    {
        if (button == null || button.targetGraphic == null)
            return;

        button.targetGraphic.color = color;
    }
}
