using UnityEngine;

public class WazardEncounterBridge : MonoBehaviour
{
    [SerializeField] private WazardDialogue wazardDialogue;
    [SerializeField] private WazardQuiz wazardQuiz;

    private void OnEnable()
    {
        if (wazardDialogue != null)
            wazardDialogue.OnWazardDialogueFinished += HandleDialogueFinished;
    }

    private void OnDisable()
    {
        if (wazardDialogue != null)
            wazardDialogue.OnWazardDialogueFinished -= HandleDialogueFinished;
    }

    private void HandleDialogueFinished()
    {
        if (wazardQuiz != null)
            wazardQuiz.StartQuiz();
    }
}
