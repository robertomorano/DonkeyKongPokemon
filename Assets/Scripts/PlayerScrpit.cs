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
    public AudioSource loopAudioSource;
    public AudioSource oneShotAudioSource;
    public AudioClip climbingSoundClip;
    [Range(0f, 1f)] public float climbingVolume = 0.7f;
    public AudioClip jumpSoundClip;
    [Range(0f, 1f)] public float jumpVolume = 1.0f;
    public AudioClip runningSoundClip;
    [Range(0f, 1f)] public float runningVolume = 0.5f;
    // >>> NUEVA VARIABLE DE AUDIO DEL MARTILLO <<<
    public AudioClip hammerAttackSoundClip;
    [Range(0f, 1f)] public float hammerAttackVolume = 0.8f;

    [Header("Martillo")]
    public float attackRange = 1.5f;    // Alcance del ataque del martillo
    public float attackCooldown = 0.5f; // Frecuencia con la que se puede golpear

    private Rigidbody2D rb;
    private Animator animator;
    private bool grounded;
    private bool climbing;
    private bool nearLadder;
    private GameObject currentLadder;
    private Collider2D playerCollider;
    private float verticalInput;
    private bool isPlayingClimbingSound = false;
    private bool isPlayingRunningSound = false;
    private bool facingRight = true;

    private int barrelLayer;
    private bool hasHammer = false;
    private float nextAttackTime = 0f;
    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;

        playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null)
        {
            Debug.LogError("¡No se encontró Collider2D en el jugador!");
        }

        // --- LÓGICA RESTAURADA DEL SPRITE RENDERER/ANIMATOR ---
        if (spriteTransform == null)
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                spriteTransform = sr.transform;
            }
        }

        if (spriteTransform != null)
        {
            animator = spriteTransform.GetComponent<Animator>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator == null)
        {
            Debug.LogWarning("¡Animator no encontrado!");
        }
        // --- FIN LÓGICA RESTAURADA ---

        barrelLayer = LayerMask.NameToLayer("Barrel");
        InitializeAudioSources();
    }

    private void InitializeAudioSources()
    {
        AudioSource[] sources = GetComponents<AudioSource>();

        // Si ya existen dos o más AudioSources, los asignamos
        if (sources.Length >= 2)
        {
            loopAudioSource = sources[0];
            oneShotAudioSource = sources[1];
        }
        else // Si faltan, los creamos
        {
            if (sources.Length == 1)
            {
                loopAudioSource = sources[0];
                oneShotAudioSource = gameObject.AddComponent<AudioSource>();
            }
            else // sources.Length == 0
            {
                loopAudioSource = gameObject.AddComponent<AudioSource>();
                oneShotAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    void Update()
    {
        LeerInput();
        DetectarSuelo();
        FlipSprite();
        ActualizarAnimator();

        if (hasHammer && !climbing && Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
        {
            PerformHammerAttack();
        }
    }

    private void FixedUpdate()
    {
        MoverJugador();
        ControlarSonidoEscalada();
        ControlarSonidoCarrera();
    }

    private void LeerInput()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        verticalInput = 0f;
        if (Input.GetKey(KeyCode.W)) verticalInput = 1f;
        else if (Input.GetKey(KeyCode.S)) verticalInput = -1f;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpPressed = true;
            PlayJumpSound();
        }
    }

    // --- Lógica del Martillo ---

    public void ActivateHammer(float duration)
    {
        if (hasHammer)
        {
            CancelInvoke(nameof(DeactivateHammer));
        }

        hasHammer = true;
        Invoke(nameof(DeactivateHammer), duration);
        Debug.Log("Martillo Recogido. Tiempo restante: " + duration + "s");
    }

    private void DeactivateHammer()
    {
        hasHammer = false;
        isAttacking = false;
        Debug.Log("Martillo Desactivado: El tiempo ha terminado.");
    }

    private void PerformHammerAttack()
    {
        if (!hasHammer) return;

        nextAttackTime = Time.time + attackCooldown;
        isAttacking = true;

        // >>> REPRODUCCIÓN DEL SONIDO DEL MARTILLO <<<
        if (oneShotAudioSource != null && hammerAttackSoundClip != null)
        {
            oneShotAudioSource.PlayOneShot(hammerAttackSoundClip, hammerAttackVolume);
        }
        // *****************************************

        Vector3 attackOrigin = transform.position;
        attackOrigin += (facingRight ? Vector3.right : Vector3.left) * (attackRange / 2f);

        int barrelLayerMask = 1 << barrelLayer;

        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackOrigin, attackRange / 2f, barrelLayerMask);

        foreach (Collider2D hit in hitObjects)
        {
            if (hit.gameObject.layer == barrelLayer)
            {
                Debug.Log("¡Martillo golpeó y destruyó a: " + hit.name + "!");
                // Se asume que el barril es un Electrode y debe detonar/destruirse
                Destroy(hit.gameObject);
            }
        }

        Debug.DrawRay(attackOrigin, facingRight ? Vector2.right * (attackRange / 2f) : Vector2.left * (attackRange / 2f), Color.red, attackCooldown);

        Invoke(nameof(ResetAttackState), 0.1f);
    }

    private void ResetAttackState()
    {
        isAttacking = false;
    }

    // --- Lógica de Movimiento y Audio ---

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
            rb.linearVelocity = new Vector2(horizontal * moveSpeed, verticalInput * climbSpeed);
            rb.gravityScale = 0f;
            IgnoreGroundCollision(true);
            jumpPressed = false;
            return;
        }

        IgnoreGroundCollision(false);
        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);
        rb.gravityScale = gravityScale;

        if (grounded && jumpPressed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpStrength);
        }

        jumpPressed = false;
    }

    private void PlayJumpSound()
    {
        if (oneShotAudioSource != null && jumpSoundClip != null)
        {
            oneShotAudioSource.PlayOneShot(jumpSoundClip, jumpVolume);
        }
    }

    // El resto de los métodos de control de sonido y movimiento se mantienen igual...

    private void ControlarSonidoEscalada()
    {
        if (loopAudioSource == null || climbingSoundClip == null) return;

        bool shouldPlay = climbing && Mathf.Abs(verticalInput) > 0.01f;

        if (shouldPlay)
        {
            if (!isPlayingClimbingSound || loopAudioSource.clip != climbingSoundClip)
            {
                if (isPlayingRunningSound) isPlayingRunningSound = false;

                loopAudioSource.clip = climbingSoundClip;
                loopAudioSource.loop = true;
                loopAudioSource.volume = climbingVolume;
                loopAudioSource.Play();
                isPlayingClimbingSound = true;
            }
        }
        else
        {
            if (isPlayingClimbingSound)
            {
                if (loopAudioSource.clip == climbingSoundClip)
                {
                    loopAudioSource.Stop();
                }
                isPlayingClimbingSound = false;
            }
        }
    }

    private void ControlarSonidoCarrera()
    {
        if (loopAudioSource == null || runningSoundClip == null) return;

        bool shouldPlay = grounded && Mathf.Abs(horizontal) > 0.01f && !climbing;

        if (shouldPlay)
        {
            if (!isPlayingRunningSound || loopAudioSource.clip != runningSoundClip)
            {
                if (isPlayingClimbingSound) return;

                loopAudioSource.clip = runningSoundClip;
                loopAudioSource.loop = true;
                loopAudioSource.volume = runningVolume;
                loopAudioSource.Play();
                isPlayingRunningSound = true;
            }
        }
        else
        {
            if (isPlayingRunningSound)
            {
                if (loopAudioSource.clip == runningSoundClip)
                {
                    loopAudioSource.Stop();
                }
                isPlayingRunningSound = false;
            }
        }
    }

    private void IgnoreGroundCollision(bool ignore)
    {
        if (playerCollider == null) return;

        Collider2D[] groundColliders = Physics2D.OverlapCircleAll(
            transform.position,
            10f,
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
        bool isClimbingAnimated = climbing && Mathf.Abs(verticalInput) > 0.01f;

        animator.SetBool("Running", isRunning);
        animator.SetBool("Climbing", isClimbingAnimated);
        // Opcional: animador.SetBool("HasHammer", hasHammer);
        // Opcional: Puedes añadir un trigger para la animación de ataque aquí:
        // if (isAttacking) animator.SetTrigger("HammerAttack");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            nearLadder = true;
            currentLadder = collision.gameObject;
            climbing = false;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            currentLadder = collision.gameObject;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
            {
                climbing = true;
                if (isPlayingRunningSound) isPlayingRunningSound = false;
            }

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
            currentLadder = null;

            IgnoreGroundCollision(false);

            if (isPlayingClimbingSound)
            {
                if (loopAudioSource.clip == climbingSoundClip)
                {
                    loopAudioSource.Stop();
                }
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

        // Dibuja el área de ataque del martillo
        if (hasHammer)
        {
            Vector3 attackOrigin = transform.position;
            attackOrigin += (facingRight ? Vector3.right : Vector3.left) * (attackRange / 2f);

            Gizmos.color = isAttacking ? Color.yellow : Color.red;
            Gizmos.DrawWireSphere(attackOrigin, attackRange / 2f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        int collidedLayer = collision.gameObject.layer;
        int killZoneLayer = LayerMask.NameToLayer("KillZone");

        if (collidedLayer == killZoneLayer || collidedLayer == barrelLayer)
        {
            // Invulnerabilidad: si tiene martillo y choca con un Barrel, no hay daño.
            if (hasHammer && collidedLayer == barrelLayer)
            {
                return;
            }

            // Recibe daño si choca con KillZone o Barrel sin martillo
            if (GameManager.Instance != null)
            {
                GameManager.Instance.HandlePlayerHit();
            }
        }
    }
}