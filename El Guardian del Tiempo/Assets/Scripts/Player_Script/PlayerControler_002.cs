using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerControler_002 : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 7f;

    [Header("Salto")]
    public float jumpForce = 11f;
    public float fallGravityMultiplier = 1.6f;
    public float lowJumpMultiplier = 2f;
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;

    [Header("Detección de Suelo")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Referencias")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private bool jumpReleased;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        // 1. Leer input
        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferTimer = jumpBufferTime;
            jumpReleased = false;
        }
        else if (Input.GetButtonUp("Jump"))
        {
            jumpReleased = true;
        }

        // 2. Comprobar suelo
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 3. Temporizadores de Coyote Time y Jump Buffer
        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        if (jumpBufferTimer > 0f)
            jumpBufferTimer -= Time.deltaTime;

        // 4. Ejecutar salto si hay buffer y coyote time disponible
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            if (animator != null) animator.SetTrigger("Jump");
        }

        // 5. Flip del sprite
        if (moveInput > 0.01f)
            spriteRenderer.flipX = false;
        else if (moveInput < -0.01f)
            spriteRenderer.flipX = true;

        // 6. Animaciones
        if (animator != null)
        {
            bool isRunning = isGrounded && Mathf.Abs(moveInput) > 0.01f;
            animator.SetBool("isRunning", isRunning);
            animator.SetBool("isGround", isGrounded);
            animator.SetBool("isFalling", !isGrounded && rb.linearVelocity.y < -0.05f);
        }
    }

    void FixedUpdate()
    {
        // Movimiento horizontal constante
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Gravedad extra al caer o al soltar el botón de salto en ascenso
        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0f && jumpReleased)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
