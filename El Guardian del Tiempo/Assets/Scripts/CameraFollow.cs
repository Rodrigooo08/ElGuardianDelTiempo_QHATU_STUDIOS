using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;   // El personaje a seguir
    public float smoothSpeed = 0.125f; // Velocidad de suavizado
    public Vector3 offset;     // Desplazamiento de la cámara respecto al personaje

    void LateUpdate()
    {
        if (target != null)
        {
            // Posición deseada de la cámara
            Vector3 desiredPosition = target.position + offset;

            // Movimiento suavizado
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            // Actualizar posición de la cámara
            transform.position = smoothedPosition;
        }
    }
}
