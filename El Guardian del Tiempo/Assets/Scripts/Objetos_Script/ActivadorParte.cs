using UnityEngine;

public class ActivadorParte : MonoBehaviour
{
    [SerializeField] private string descripcionParte; // parte1, parte2, parte3
    [SerializeField] private KeyCode teclaInteractiva = KeyCode.E;
    private ManagerObjetivos panelObjetivo;
    private bool playerEnZona;

    public void Start()
    {
        panelObjetivo = FindAnyObjectByType<ManagerObjetivos>();
    }
    private void Update()
    {
        if (playerEnZona && Input.GetKeyDown(teclaInteractiva))
        {
            panelObjetivo.MarcarCumplido(descripcionParte); //marcar objetivo cumplido

            Destroy(gameObject);
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
