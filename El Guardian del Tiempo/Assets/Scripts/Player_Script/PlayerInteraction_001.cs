using UnityEngine;

public class PlayerInteraction_001 : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, 15f);
            if (hit.collider != null)
            {
                IInteractuable interactable = hit.collider.GetComponent<IInteractuable>();
                if (interactable != null)
                {
                    interactable.Interact();
                }
            }
        }
    }
}
