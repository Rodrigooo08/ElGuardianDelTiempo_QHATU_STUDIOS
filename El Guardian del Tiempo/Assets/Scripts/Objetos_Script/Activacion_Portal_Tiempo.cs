using UnityEngine;
using UnityEngine.SceneManagement;

public class Activacion_Portal_Tiempo : MonoBehaviour
{
    [SerializeField] private KeyCode teclaInteractiva = KeyCode.E;
    [SerializeField] string nombreEscena;
    private bool playerEnZona;

    private void Update()
    {
        if (playerEnZona && Input.GetKeyDown(teclaInteractiva))
        {
            if (TransicionEscena.Instancia != null)
            {
                TransicionEscena.Instancia.IniciarTransicion(nombreEscena);
            }
            else
            {
                Debug.LogError("TransicionEscena no está inicializado. Asegúrate de que el objeto está presente en la escena.");
            }
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
