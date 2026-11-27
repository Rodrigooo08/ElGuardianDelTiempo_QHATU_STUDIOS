using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FinalCutsceneController : MonoBehaviour
{
    [Serializable]
    public struct DialogueEntry
    {
        public string speaker;
        [TextArea(3, 5)] public string line;
    }

    [Header("Panel de Diálogo Existente")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerText; // Opcional: dejalo vacío si incluís el nombre en la línea
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private float typingTime = 0.04f;
    [SerializeField] private KeyCode advanceKey = KeyCode.E;

    [Header("Líneas de la Escena Final (Versión Corta)")]
    [SerializeField]
    private DialogueEntry[] lines = new DialogueEntry[]
    {
        new DialogueEntry
        {
            speaker = "ROLLY",
            line = "Ya devolvimos cada momento a su lugar. Pero antes de irme... ¿por qué hacías todo esto desde las sombras? Preparabas cada escena para quedar vos como el salvador."
        },
        new DialogueEntry
        {
            speaker = "WAZARD",
            line = "Porque creí que si me ponía en el centro como el que salva todo, por fin alguien me iba a mirar. Toda mi vida estuve ahí, detrás... esperando que notaran que existía. Y nunca pasaba."
        },
        new DialogueEntry
        {
            speaker = "ROLLY",
            line = "Te sentiste solo. Duele mucho sentir que nadie te ve o que estás de más."
        },
        new DialogueEntry
        {
            speaker = "WAZARD",
            line = "...Me llamo Wazard. Yo solo quería que, por una vez, me eligieran y me reconocieran. Que no me dejaran a un costado."
        },
        new DialogueEntry
        {
            speaker = "ROLLY",
            line = "Querer que te vean no está mal, Wazard. Lo que lastima es borrar a otros para brillar. Acá hay lugar para todos."
        },
        new DialogueEntry
        {
            speaker = "WAZARD",
            line = "...Gracias por verme, Rolly."
        }
    };

    [Header("Imágenes Estática y Apreton de manos")]
    [SerializeField] private GameObject staticSceneObject;   // Imagen estática inicial (activa al empezar)
    [SerializeField] private GameObject handshakeSceneObject; // Primer plano del apretón de manos (inactiva al empezar)

    [Header("Frase Final")]
    [SerializeField] private GameObject finalQuotePanel;
    [SerializeField] private TMP_Text finalQuoteText;
    [SerializeField, TextArea(2, 4)]
    private string finalQuote =
        "La historia no solo se recuerda: también se construye ayudando a quienes hoy se sienten solos o excluidos.";
    [SerializeField] private float delayBeforeFinalQuote = 0.4f;

    [Header("3. Panel de Créditos y Scroll")]
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private TMP_Text creditsText;
    [SerializeField] private RectTransform creditsContentRect;
    [SerializeField] private float scrollSpeed = 45f; // Píxeles por segundo
    [SerializeField] private float startOffsetY = -500f;
    [SerializeField] private float endOffsetY = 900f;
    [SerializeField] private bool loopCredits = false;

    [TextArea(18, 28)]
    [SerializeField]
    private string creditsContent =
@"¡Gracias por jugar!

Lucas Fabrego:
Director de Proyecto
Productor
Programador Unity 2
Diseñador de Juegos 1

Axel Jurado:
Artista Gráfico
Diseñador de Sonido
Diseñador de Niveles 2

Rodrigo Fines:
Programador Unity 1
Diseñador de Juegos 2
Diseñador de Niveles 1
Especialista en Marketing

Cintia Flores:
Historiador
Guionista
Tester

Esteban Ibáñez:
Animador
Diseñador de UI
Especialista en Marketing 2";

    [Header("Opciones de Inicio")]
    [SerializeField] private bool playOnStart = true;

    public event Action OnFinalSceneCompleted;
    private enum SceneStep
    {
        Dialogue,
        FinalQuote,
        Credits
    }

    private SceneStep currentStep = SceneStep.Dialogue;
    private int currentIndex;
    private bool isPlaying;
    private bool isScrollingCredits;
    private Coroutine typingCoroutine;

    private void Start()
    {
        SetInitialStates();

        if (playOnStart)
        {
            StartFinalScene();
        }
    }
    private void SetInitialStates()
    {
        if (staticSceneObject != null) staticSceneObject.SetActive(true);
        if (handshakeSceneObject != null) handshakeSceneObject.SetActive(false);
        if (finalQuotePanel != null) finalQuotePanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (!isPlaying) return;
        if (currentStep == SceneStep.Credits && isScrollingCredits)
        {
            UpdateCreditsScroll();
        }

        if (Input.GetKeyDown(advanceKey) || Input.GetMouseButtonDown(0))
        {
            if (currentStep == SceneStep.Dialogue)
            {
                string fullLine = lines[currentIndex].line;
                if (dialogueText != null && dialogueText.text != fullLine)
                {
                    CompleteCurrentLineInstantly();
                }
                else
                {
                    AdvanceDialogue();
                }
            }
            else if(currentStep == SceneStep.FinalQuote)
            {
                StartCreditsPanel();
            }
            else if (currentStep == SceneStep.Credits)
            {
                // Acelera un poco o finaliza si se presiona en los créditos
                FinishScene();
            }
        }
    }

    public void StartFinalScene()
    {
        if (lines == null || lines.Length == 0) return;

        Time.timeScale = 1f;
        isPlaying = true;
        isScrollingCredits = false;
        currentIndex = 0;

        SetInitialStates();
        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        DialogueEntry entry = lines[currentIndex];
        if (speakerText != null)
        {
            speakerText.text = entry.speaker;
        }

        typingCoroutine = StartCoroutine(TypeLine(entry));
    }

    private void CompleteCurrentLineInstantly()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        DialogueEntry entry = lines[currentIndex];
        dialogueText.text = speakerText != null ? entry.line : $"<b>{entry.speaker}:</b> {entry.line}";
    }

    private void AdvanceDialogue()
    {
        currentIndex++;

        if (currentIndex < lines.Length)
        {
            ShowCurrentLine();
        }
        else
        {
            StartCoroutine(ShowHandshakeAndFinalQuote());
        }
    }

    private IEnumerator TypeLine(DialogueEntry entry)
    {
        dialogueText.text = speakerText != null ? string.Empty : $"<b>{entry.speaker}:</b> ";
        string prefix = dialogueText.text;

        foreach (char c in entry.line)
        {
            dialogueText.text += c;
            yield return new WaitForSecondsRealtime(typingTime);
        }

        dialogueText.text = prefix + entry.line;
        typingCoroutine = null;
    }

    private IEnumerator ShowHandshakeAndFinalQuote()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        currentStep = SceneStep.FinalQuote;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // Cambia la imagen estática por el primer plano del apretón de manos
        if (staticSceneObject != null) staticSceneObject.SetActive(false);
        if (handshakeSceneObject != null) handshakeSceneObject.SetActive(true);

        if (delayBeforeFinalQuote > 0f)
        {
            yield return new WaitForSecondsRealtime(delayBeforeFinalQuote);
        }

        if (finalQuoteText != null)
        {
            finalQuoteText.text = $"\"{finalQuote}\"";
        }

        if (finalQuotePanel != null)
        {
            finalQuotePanel.SetActive(true);
        }
    }
    public void StartCreditsPanel()
    {
        currentStep = SceneStep.Credits;

        if (finalQuotePanel != null) finalQuotePanel.SetActive(false);
        if (handshakeSceneObject != null) handshakeSceneObject.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(true);

        if (creditsText != null)
            creditsText.text = creditsContent;

        if (creditsContentRect != null)
        {
            Vector2 pos = creditsContentRect.anchoredPosition;
            pos.y = startOffsetY;
            creditsContentRect.anchoredPosition = pos;
        }

        isScrollingCredits = true;
    }
    private void UpdateCreditsScroll()
    {
        if (creditsContentRect == null) return;

        Vector2 pos = creditsContentRect.anchoredPosition;
        pos.y += scrollSpeed * Time.unscaledDeltaTime;
        creditsContentRect.anchoredPosition = pos;

        if (pos.y >= endOffsetY)
        {
            if (loopCredits)
            {
                pos.y = startOffsetY;
                creditsContentRect.anchoredPosition = pos;
            }
            else
            {
                isScrollingCredits = false;
                FinishScene();
            }
        }
    }
    public void FinishScene()
    {
        if (!isPlaying) return;
        isPlaying = false;
        OnFinalSceneCompleted?.Invoke();
    }
}
