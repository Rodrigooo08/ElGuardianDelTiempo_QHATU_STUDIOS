using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private Transform destino;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.transform.position = destino.position;
        }
    }
}
