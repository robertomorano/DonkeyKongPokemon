using UnityEngine;

public class Player : MonoBehaviour
{
    private float horizontal;
    private bool jumpPressed;

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpStrength = 8f;
    [SerializeField] private float gravityScale = 3f;
    [SerializeField] private float climbSpeed = 1.5f;

    [Header("Referencias")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.18f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform spriteTransform;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip climbingSoundClip;
    [Tooltip("Volumen de reproducción del sonido de escalada (0.0 a 1.0).")]
    [Range(0f, 1f)]
    public float climbingVolume = 0.7f; // Volumen por defecto

    private Rigidbody2D rb;
    private Animator animator;
    private bool grounded;
    private bool climbing;
    private bool nearLadder;
    private GameObject currentLadder;
    private Collider2D playerCollider;

    // Variables de control de audio y movimiento vertical
    private float verticalInput;
    private bool isPlayingClimbingSound = false;

    private bool facingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;

        // Obtener el collider del jugador
        playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null)
        {
            Debug.LogError("¡No se encontró Collider2D en el jugador!");
        }

        if (spriteTransform == null)
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
                spriteTransform = sr.transform;
        }

        if (spriteTransform != null)
            animator = spriteTransform.GetComponent<Animator>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
            Debug.LogWarning("¡Animator no encontrado!");

        // Inicialización y configuración de AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        if (audioSource != null && climbingSoundClip != null)
        {
            audioSource.clip = climbingSoundClip;
            audioSource.loop = true; // El sonido de escalada debe repetirse
            // El volumen inicial se establecerá en la función de control
        }
    }

    void Update()
    {
        LeerInput();
        DetectarSuelo();
        FlipSprite();
        ActualizarAnimator();
    }

    private void FixedUpdate()
    {
        MoverJugador();
        ControlarSonidoEscalada();
    }

    private void LeerInput()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        // Capturamos el input vertical aquí
        verticalInput = 0f;
        if (Input.GetKey(KeyCode.W)) verticalInput = 1f;
        else if (Input.GetKey(KeyCode.S)) verticalInput = -1f;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpPressed = true;
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

    private void MoverJugador()
    {
        if (climbing && currentLadder != null)
        {
            // Movimiento en la escalera
            rb.linearVelocity = new Vector2(horizontal * moveSpeed, verticalInput * climbSpeed);
            rb.gravityScale = 0f;

            // Desactivar colisiones con el suelo mientras escala
            IgnoreGroundCollision(true);

            // No permitir salto normal mientras escala
            jumpPressed = false;
            return;
        }

        // Reactivar colisiones con el suelo cuando NO está escalando
        IgnoreGroundCollision(false);

        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);
        rb.gravityScale = gravityScale;

        if (grounded && jumpPressed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpStrength);
        }

        jumpPressed = false;
    }

    // Función para controlar la reproducción del sonido
    private void ControlarSonidoEscalada()
    {
        if (audioSource == null || climbingSoundClip == null) return;

        // Solo reproducir si está escalando Y hay movimiento vertical
        bool shouldPlay = climbing && Mathf.Abs(verticalInput) > 0.01f;

        if (shouldPlay && !isPlayingClimbingSound)
        {
            audioSource.volume = climbingVolume; // Establecer el volumen
            audioSource.Play();
            isPlayingClimbingSound = true;
        }
        else if (!shouldPlay && isPlayingClimbingSound)
        {
            audioSource.Stop();
            isPlayingClimbingSound = false;
        }
    }

    // Método para ignorar/reactivar colisiones con el suelo
    private void IgnoreGroundCollision(bool ignore)
    {
        if (playerCollider == null) return;

        // Obtener todos los colliders del layer de suelo
        Collider2D[] groundColliders = Physics2D.OverlapCircleAll(
            transform.position,
            10f, // Radio de búsqueda amplio
            groundLayer
        );

        foreach (Collider2D groundCol in groundColliders)
        {
            Physics2D.IgnoreCollision(playerCollider, groundCol, ignore);
        }
    }

    private void FlipSprite()
    {
        if (spriteTransform == null) return;

        Vector3 currentScale = spriteTransform.localScale;

        if (horizontal > 0f)
        {
            currentScale.x = Mathf.Abs(currentScale.x);
            facingRight = true;
        }
        else if (horizontal < 0f)
        {
            currentScale.x = -Mathf.Abs(currentScale.x);
            facingRight = false;
        }

        spriteTransform.localScale = currentScale;
    }

    private void ActualizarAnimator()
    {
        if (animator == null) return;

        bool isRunning = horizontal != 0f && grounded;

        // Solo animar la escalada si se está moviendo verticalmente
        bool isClimbingAnimated = climbing && Mathf.Abs(verticalInput) > 0.01f;

        animator.SetBool("Running", isRunning);
        animator.SetBool("Climbing", isClimbingAnimated);
    }

    // Detectar escaleras usando trigger y GameObject
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            nearLadder = true;
            currentLadder = collision.gameObject;
            Debug.Log("Cerca de una escalera");
            climbing = false; // se activará al presionar W/S
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            currentLadder = collision.gameObject;

            // Activar climbing al presionar W o S por primera vez
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
            {
                climbing = true;
            }

            // Desactivar climbing solo si presiona Space (saltar) para salir de la escalera
            if (Input.GetKeyDown(KeyCode.Space) && climbing)
            {
                climbing = false;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            nearLadder = false;
            climbing = false;
            Debug.Log("Fuera de la escalera");
            currentLadder = null;

            // Asegurar que las colisiones se reactiven al salir
            IgnoreGroundCollision(false);

            // Detener el sonido al salir del trigger
            if (isPlayingClimbingSound)
            {
                audioSource.Stop();
                isPlayingClimbingSound = false;
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("KillZone"))
        {
            // Asegúrate de que GameManager.Instance esté definido o elimina esta línea si no lo usas
            // GameManager.Instance.HandlePlayerHit(); 

        }
    }
}