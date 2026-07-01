using UnityEngine;
using UnityEngine.UI;

public class ParallaxLayer : MonoBehaviour
{
    public float parallaxFactor = 0.5f; // velocidad relativa
    public RectTransform imageA;
    public RectTransform imageB;

    private float imageWidth;
    private Transform cameraTransform;
    private Vector3 lastCameraPosition;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;
        imageWidth = imageA.rect.width; // ancho en UI
    }

    void Update()
    {
        // Movimiento parallax
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        transform.localPosition += new Vector3(deltaMovement.x * parallaxFactor, 0, 0);
        lastCameraPosition = cameraTransform.position;

        // Loop infinito
        if (cameraTransform.position.x > imageB.position.x)
        {
            imageA.localPosition += new Vector3(imageWidth * 2, 0, 0);
            SwapImages();
        }
        else if (cameraTransform.position.x < imageA.position.x)
        {
            imageB.localPosition -= new Vector3(imageWidth * 2, 0, 0);
            SwapImages();
        }
    }

    void SwapImages()
    {
        // Intercambia referencias para mantener el orden
        var temp = imageA;
        imageA = imageB;
        imageB = temp;
    }
}
