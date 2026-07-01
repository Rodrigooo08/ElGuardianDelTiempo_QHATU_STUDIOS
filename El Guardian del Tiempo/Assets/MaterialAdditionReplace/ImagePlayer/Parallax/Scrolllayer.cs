using UnityEngine;

public class Scrolllayer : MonoBehaviour
{
    [SerializeField] private Transform cam;
    [SerializeField] private float parallaxMultipler = 0.5f;

    private float spriteWidth;
    private float lastCamPos;

    private Transform[] backgrounds = new Transform[2];

    private void Start()
    {
        for (int i = 0; i < 2; i++)
        {
            backgrounds[i] = transform.GetChild(i);
        }
    }
}
