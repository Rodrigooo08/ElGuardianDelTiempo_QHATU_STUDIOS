using UnityEngine;

public class RollyController : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;

    [Header("Salto")]
    public float fuerzaSalto = 10f;

    private Rigidbody2D rb;
    private bool enSuelo = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Movimiento izquierda y derecha
        float movimiento = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(movimiento * velocidad, rb.linearVelocity.y);

        // Salto
        if (Input.GetButtonDown("Jump") && enSuelo)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
            enSuelo = false;
        }
    }

    // Detecta cuando toca el suelo
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
            enSuelo = true;
        }
    }
}