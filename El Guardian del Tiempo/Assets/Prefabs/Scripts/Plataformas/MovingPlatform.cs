using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform puntoA;
    public Transform puntoB;
    public float velocidad = 2f;
    [SerializeField]
    private Transform objetivo;

  
    void Start()
    {
        objetivo = puntoB;

    }

    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, objetivo.position, velocidad * Time.deltaTime);

        if (Vector2.Distance(transform.position, objetivo.position) < 0.05f)
        {
            objetivo = (objetivo == puntoA) ? puntoB : puntoA;
        }
    }
}