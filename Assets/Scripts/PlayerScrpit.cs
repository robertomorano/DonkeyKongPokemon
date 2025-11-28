using UnityEngine;
using System;

public class Player : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public Sprite[] runSprites;
    public Sprite climbSprite;
    private int spriteIndex;

    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;

    // Arrays de colisi�n espec�ficos para evitar el error de conversi�n:
    private readonly RaycastHit2D[] rayHits = new RaycastHit2D[4];  // Para BoxCast (Detecci�n de suelo)
    private readonly Collider2D[] overlapHits = new Collider2D[4]; // Para OverlapBox (Detecci�n de escalera)

    private float horizontalInput;
    private float verticalInput;

    private bool grounded;
    private bool climbing;

    public float moveSpeed = 6f;
    public float jumpStrength = 8f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    private void OnEnable()
    {
        // Llama a AnimateSprite repetidamente
        InvokeRepeating(nameof(AnimateSprite), 1f / 12f, 1f / 12f);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void Update()
    {
        // 1. Capturar la entrada del usuario
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        // 2. Comprobar colisiones para establecer los estados (grounded/climbing)
        CheckCollision();
        grounded = true;
        // 3. Manejar el salto
        if (grounded && Input.GetButtonDown("Jump"))
        {
            // Aplicar la fuerza de salto directamente a la velocidad Y
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpStrength);
        }

        // 4. Rotar el sprite (basado en la intenci�n de movimiento)
        if (horizontalInput > 0.01f)
        {
            transform.eulerAngles = Vector3.zero;
        }
        else if (horizontalInput < -0.01f)
        {
            transform.eulerAngles = new Vector3(0f, 180f, 0f);
        }
    }

    private void FixedUpdate()
    {
        // 5. Aplicar el movimiento en FixedUpdate para la f�sica
        ApplyMovement();
    }

    // --- L�GICA DE MOVIMIENTO ---

    private void ApplyMovement()
    {
        Vector2 velocity = rb.linearVelocity;

        if (climbing)
        {
            // Desactivar la gravedad y controlar la velocidad Y con la entrada
            rb.gravityScale = 0f;
            velocity.y = verticalInput * moveSpeed;

            // Si no hay entrada, detenerse en la escalera
            if (Mathf.Abs(verticalInput) < 0.01f && Mathf.Abs(horizontalInput) < 0.01f)
            {
                velocity.y = 0f;
            }
        }
        else
        {
            // Restablecer la gravedad cuando no est� escalando
            rb.gravityScale = 1f; // Aseg�rate de que este sea el valor predeterminado
        }

        // Aplicar el movimiento horizontal (siempre se puede mover horizontalmente)
        velocity.x = horizontalInput * moveSpeed;

        // Establecer la nueva velocidad
        rb.linearVelocity = velocity;
    }

    // --- L�GICA DE DETECCI�N DE COLISI�N ---

    private void CheckCollision()
    {
        grounded = false;
        climbing = false;

        float skinWidth = 0.05f;
        Vector2 size = capsuleCollider.size;
        Vector2 origin = rb.position + capsuleCollider.offset;

        // 1. Detecci�n de suelo (BoxCastNonAlloc -> requiere RaycastHit2D[])
        // Hacemos el BoxCast ligeramente m�s estrecho para evitar colisiones laterales
        size.x *= 0.9f;

        int groundHits = Physics2D.BoxCastNonAlloc(
            origin,
            size,
            0f,
            Vector2.down, // Direcci�n hacia abajo
            rayHits, // Array de salida
            skinWidth,
            LayerMask.GetMask("Ground")
        );

        // Si golpe� algo y no estamos saltando
        if (groundHits > 0 && rb.linearVelocity.y <= 0.01f)
        {
            for (int i = 0; i < groundHits; i++)
            {
                // Simple comprobaci�n de que el punto de contacto est� debajo del jugador
                if (rayHits[i].point.y < origin.y)
                {
                    grounded = true;
                    break;
                }
            }
        }

        // 2. Detecci�n de escaleras (OverlapBoxNonAlloc -> requiere Collider2D[])
        int ladderOverlap = Physics2D.OverlapBoxNonAlloc(
            origin,
            capsuleCollider.size,
            0f,
            overlapHits, // Array de salida (Collider2D[])
            LayerMask.GetMask("Ladder")
        );

        if (ladderOverlap > 0)
        {
            climbing = true;
            grounded = false; // No puede estar en tierra y escalando a la vez
        }
    }

    // --- L�GICA DE ANIMACI�N ---

    private void AnimateSprite()
    {
        if (climbing)
        {
            spriteRenderer.sprite = climbSprite;
        }
        else if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            // Animaci�n de correr
            spriteIndex++;

            if (spriteIndex >= runSprites.Length)
            {
                spriteIndex = 0;
            }

            spriteRenderer.sprite = runSprites[spriteIndex];
        }
        else
        {
            // El jugador est� quieto, muestra el primer sprite de correr (Idle)
            if (runSprites.Length > 0)
            {
                spriteRenderer.sprite = runSprites[0];
            }
        }
    }

    // --- L�GICA DE EVENTOS DE COLISI�N ---

    /* private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Objective"))
        {
            enabled = false;
            // GameManager.Instance.LevelComplete();
        }
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            enabled = false;
            // GameManager.Instance.LevelFailed();
        }
    }
    */
}