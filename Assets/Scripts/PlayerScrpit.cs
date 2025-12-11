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

    private Rigidbody2D rb;
    private Animator animator;
    private bool grounded;
    private bool climbing;
    private bool nearLadder;
    private GameObject currentLadder;


    private bool facingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;

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
    }

    private void LeerInput()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

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
            float verticalInput = 0f;
            if (Input.GetKey(KeyCode.W)) verticalInput = 1f;
            else if (Input.GetKey(KeyCode.S)) verticalInput = -1f;

            rb.linearVelocity = new Vector2(horizontal * moveSpeed, verticalInput * climbSpeed);
            rb.gravityScale = 0f;
            jumpPressed = false;
            return;
        }

        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);
        rb.gravityScale = gravityScale;

        if (grounded && jumpPressed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpStrength);
        }

        jumpPressed = false;
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
        bool isJumping = !grounded && !climbing;

        animator.SetBool("Running", isRunning);
        animator.SetBool("Climbing", climbing);
        animator.SetBool("jumping", isJumping);
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
            Debug.Log("En la escalera");

            // Activar climbing solo si se presiona W o S
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
            {
                climbing = true;
            }
            else
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
