using UnityEngine;

public class NpcPromptTrigger : MonoBehaviour
{
    [SerializeField] private GameObject promptObject;
    [SerializeField] private string playerTag = "Player";

    private bool isPlayerInRange;
    private bool isInteracting;

    private void Awake()
    {
        if (promptObject != null)
        {
            promptObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isPlayerInRange && !isInteracting && Input.GetKeyDown(KeyCode.E))
        {
            isInteracting = true;
            SetPromptVisible(false);
        }
    }

    public void SetInteractingState(bool interacting)
    {
        isInteracting = interacting;
        SetPromptVisible(isPlayerInRange && !isInteracting);
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptObject != null && promptObject.activeSelf != visible)
        {
            promptObject.SetActive(visible);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        isPlayerInRange = true;
        if (!isInteracting)
        {
            SetPromptVisible(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        isPlayerInRange = false;
        SetPromptVisible(false);
    }
}
