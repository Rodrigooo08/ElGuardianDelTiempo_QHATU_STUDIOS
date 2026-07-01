using UnityEngine;

public class Chest : MonoBehaviour, IInteractuable 
{
    public GameObject hiddenObject;

    public void Interact()
    {
        Debug.Log("Cofre Abierto");
        if (hiddenObject != null)
        {
            hiddenObject.SetActive(true); //muestra el objeto oculto
        }
    }
}
