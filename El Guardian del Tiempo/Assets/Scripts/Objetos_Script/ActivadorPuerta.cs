using UnityEngine;

public class ActivadorPuerta : MonoBehaviour
{ 
    [SerializeField] private KeyCode teclaInteractiva = KeyCode.E;
    [SerializeField] GameObject panelLevelCompleto;
    private bool playerEnZona;

    private void Update()
    {
        if (playerEnZona && Input.GetKeyDown(teclaInteractiva))
        {
            panelLevelCompleto.SetActive(true);

        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerEnZona = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerEnZona = false;

    }
}
