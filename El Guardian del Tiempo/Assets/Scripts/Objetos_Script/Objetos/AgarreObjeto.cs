using UnityEngine;

public class AgarreObjeto : MonoBehaviour
{

    [SerializeField] private KeyCode teclaInteractiva = KeyCode.E;
    private bool playerEnZona;

    public void Start()
    {
       
    }
    private void Update()
    {
        if (playerEnZona && Input.GetKeyDown(teclaInteractiva))
        {
            

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
