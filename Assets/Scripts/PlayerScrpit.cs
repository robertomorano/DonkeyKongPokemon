using UnityEngine;

public class Player : MonoBehaviour
{
    private float horizontal;
    private float vertical;
    private bool jumpPressed;

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpStrength = 8f;
    [SerializeField] private float gravityScale = 3f;

    [Header("Referencias")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.18f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform spriteTransform;

    private Rigidbody2D rb;
    private Animator animator;
    private bool grounded;
    private bool climbing;
    private int Vida = 3;

    // Para controlar el flip sin afectar el scale
    private bool facingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;

        // Si no asignaste spriteTransform en el inspector
        if (spriteTransform == null)
        {
            // Busca el hijo con SpriteRenderer
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
                spriteTransform = sr.transform;
        }

        // Buscar el Animator en el mismo objeto que el SpriteRenderer
        if (spriteTransform != null)
        {
            animator = spriteTransform.GetComponent<Animator>();
        }

        // Si aún no lo encuentra, intentar buscarlo en el Player
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // Verificar que todo esté asignado
        if (animator == null)
            Debug.LogWarning("¡Animator no encontrado! Asegúrate de tener un Animator en el objeto con el SpriteRenderer");
    }

    void Update()
    {
        LeerInput();
        DetectarSuelo();
        DetectarEscaleras();
        FlipSprite();
        ActualizarAnimator(); // Mover DESPUÉS de FlipSprite

        // Debug para verificar valores
        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log($"Horizontal: {horizontal}, Grounded: {grounded}, Animator: {(animator != null ? "OK" : "NULL")}");
        }
    }

    private void FixedUpdate()
    {
        MoverJugador();
    }

    private void LeerInput()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        // Leer espacio para saltar
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpPressed = true;
            Debug.Log("Espacio presionado!");
        }
    }

    private void DetectarSuelo()
    {
        if (groundCheck != null)
        {
            grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
        }
        else
        {
            Debug.LogWarning("GroundCheck no asignado!");
        }
    }

    private void DetectarEscaleras()
    {
        climbing = Physics2D.Raycast(transform.position, Vector2.up, 0.3f, LayerMask.GetMask("Ladder"));

        if (climbing)
            rb.gravityScale = 0f;
        else
            rb.gravityScale = gravityScale;
    }

    private void MoverJugador()
    {
        if (climbing)
        {
            rb.linearVelocity = new Vector2(horizontal * moveSpeed, vertical * moveSpeed);
            jumpPressed = false; // Resetear salto si está escalando
            return;
        }

        // Movimiento horizontal
        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);

        // Salto
        if (grounded && jumpPressed)
        {
            Debug.Log("¡Saltando!");
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpStrength);
        }

        // Resetear el salto después de procesarlo
        jumpPressed = false;
    }

    private void FlipSprite()
    {
        if (spriteTransform == null) return;

        // Obtener el scale actual
        Vector3 currentScale = spriteTransform.localScale;

        // Girar a la derecha
        if (horizontal > 0f)
        {
            currentScale.x = Mathf.Abs(currentScale.x); // Asegurar que sea positivo
            facingRight = true;
        }
        // Girar a la izquierda
        else if (horizontal < 0f)
        {
            currentScale.x = -Mathf.Abs(currentScale.x); // Asegurar que sea negativo
            facingRight = false;
        }

        spriteTransform.localScale = currentScale;
    }

    private void ActualizarAnimator()
    {
        if (animator == null)
        {
            Debug.LogError("¡Animator es NULL! Verifica que esté asignado.");
            return;
        }

        // Verificar que el animator esté habilitado
        if (!animator.enabled)
        {
            Debug.LogError("¡Animator está deshabilitado!");
            animator.enabled = true;
        }

        bool isRunning = horizontal != 0f && grounded;
        bool isJumping = !grounded && !climbing;

        // Establecer los parámetros
        animator.SetBool("Running", isRunning);
        animator.SetBool("Climbing", climbing);
        animator.SetBool("jumping", isJumping);

        // Debug cada segundo para no llenar la consola
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"Animator UPDATE - Running: {isRunning}, Climbing: {climbing}, Jumping: {isJumping}");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Vida--;
            Debug.Log($"¡Golpe! Vida restante: {Vida}");

            if (Vida <= 0)
            {
                Debug.Log("Jugador ha muerto");
                // Aquí podrías agregar: Destroy(gameObject);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = grounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }
}