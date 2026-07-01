using System.Collections;
using UnityEngine;

public class InteracionObjeto : MonoBehaviour, IInteractuable
{
    public GameObject hiddenObject;

    public void Interact()
    {
        Debug.Log("Cofre abierto!");
        if (hiddenObject != null)
        {
            hiddenObject.SetActive(true); // libera el objeto oculto
        }
        StartCoroutine(OcultarCofre());
    }

    public void OnTriggerEnter2D (Collider2D other)
    {
        Debug.Log("Objeto Colision" + other.gameObject.tag);
        if (other.gameObject.CompareTag("Player")) 
            {
                Interact();
            }
    }

    private IEnumerator OcultarCofre()
    {
        yield return new WaitForSeconds(2f);
        Debug.Log("El cofre desaparecera");
        gameObject.SetActive(false);
    }
}
