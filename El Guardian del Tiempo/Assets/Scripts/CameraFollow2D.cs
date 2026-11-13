using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;       // El personaje a seguir
    public Vector3 offset = new Vector3(0f, 1.5f, -10f); // Offset recomendado

    [Header("Suavizado")]
    public float smoothTimeX = 0.15f; // Tiempo de respuesta en X
    public float smoothTimeY = 0.15f; // Tiempo de respuesta en Y

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        // Posición deseada basada en el objetivo y el offset
        Vector3 targetPosition = target.position + offset;

        // Movimiento suave independiente para X e Y usando SmoothDamp
        float smoothedX = Mathf.SmoothDamp(transform.position.x, targetPosition.x, ref velocity.x, smoothTimeX);
        float smoothedY = Mathf.SmoothDamp(transform.position.y, targetPosition.y, ref velocity.y, smoothTimeY);

        // Mantener la Z fija del offset
        transform.position = new Vector3(smoothedX, smoothedY, targetPosition.z);
    }
}
