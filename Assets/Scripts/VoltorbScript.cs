using UnityEngine;
using System.Collections.Generic;

public class VoltorbScript : MonoBehaviour
{
    // --- Configuración Pública ---
    [Header("Movement")]
    public float rollSpeed = 3f;
    [Range(0f, 1f)]
    public float ladderDescentChance = 0.4f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip rollingSoundClip;
    [Tooltip("Distancia hacia abajo para comprobar si está tocando el suelo.")]
    public float groundCheckDistance = 0.6f; // Ajusta este valor según el tamaño del colisionador

    // --- Componentes y Estado ---
    public Rigidbody2D rb;

    [Tooltip("Dirección horizontal actual de rodadura (-1: Izquierda, 1: Derecha)")]
    public float horizontalDirection = -1f; // Iniciar rodando hacia la izquierda
    private bool isRollingDownLadder = false;

    // Nuevo estado para la lógica de sonido
    private bool isGrounded = false;

    // --- Capas y Constantes ---
    private int groundLayer;
    private int playerLayer;
    private int killZoneLayer;
    private int inverterWallLayer;
    private const float OVERLAP_CHECK_RADIUS = 0.5f;

    // Colisionadores de suelo ignorados durante el descenso por escalera
    private readonly List<Collider2D> ignoredGroundColliders = new List<Collider2D>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        InitializeLayers();
        // Asegurarse de que el AudioSource esté configurado si no está asignado en el Inspector
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                // Si no hay AudioSource, añadir uno
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    private void InitializeLayers()
    {
        // Se inicializan igual que antes
        groundLayer = LayerMask.NameToLayer("Ground");
        playerLayer = LayerMask.NameToLayer("Player");
        killZoneLayer = LayerMask.NameToLayer("KillZone");
        inverterWallLayer = LayerMask.NameToLayer("Wall");
    }

    private void Start()
    {
        // Establecer la velocidad inicial de rodadura
        UpdateHorizontalVelocity();

        // Configurar el AudioSource para el sonido de rodadura
        if (audioSource != null && rollingSoundClip != null)
        {
            audioSource.clip = rollingSoundClip;
            audioSource.loop = true; // El sonido debe repetirse mientras rueda
            // No reproducir al inicio, se hará en FixedUpdate
        }
    }

    private void FixedUpdate()
    {
        // 1. Comprobar el estado de estar en el suelo
        CheckIfGrounded();

        // 2. Actualizar la reproducción del sonido de rodadura
        UpdateRollingSound();

        // Aseguramos que la velocidad horizontal sea correcta si no está en descenso por escalera
        if (!isRollingDownLadder)
        {
            UpdateHorizontalVelocity();
        }
    }


    // --- Lógica de Suelo y Sonido ---

    private void CheckIfGrounded()
    {
        // Usar un Raycast o BoxCast pequeño hacia abajo para verificar si está tocando el suelo.
        // Se usa el bitwise OR (|) para crear una máscara de capa solo con la capa "Ground".
        int layerMask = 1 << groundLayer;

        // Se usa Physics2D.Raycast: empieza en el centro, va hacia abajo por 'groundCheckDistance'.
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, layerMask);

        // Si el raycast golpea algo en la capa de suelo, está en el suelo.
        isGrounded = hit.collider != null;

        // Visualización del Raycast (solo en el editor)
        // Debug.DrawRay(transform.position, Vector2.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
    }

    private void UpdateRollingSound()
    {
        // Rodando si está en el suelo Y moviéndose horizontalmente, Y no está descendiendo por la escalera (que detiene el movimiento horizontal)
        bool shouldBeRolling = isGrounded && Mathf.Abs(rb.linearVelocity.x) > 0.01f && !isRollingDownLadder;

        if (audioSource == null || rollingSoundClip == null)
        {
            return; // No hay audio para reproducir
        }

        if (shouldBeRolling && !audioSource.isPlaying)
        {
            // Debería rodar, pero el sonido está detenido -> Empezar a rodar
            audioSource.Play();
        }
        else if (!shouldBeRolling && audioSource.isPlaying)
        {
            // No debería rodar, pero el sonido se está reproduciendo -> Detener la rodadura
            audioSource.Stop();
        }
    }

    // --- Métodos de Colisión y Trigger (Mantener los métodos existentes) ---
    // ... (El resto de los métodos OnCollisionEnter2D, OnTriggerEnter2D, etc. se mantienen)


    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject collidedObject = collision.gameObject;
        int collidedLayer = collidedObject.layer;

        if (HandleDestructiveCollisions(collidedLayer, collidedObject))
        {
            return;
        }

        // 1. Manejar colisión con el suelo (Aterrizajes)
        // Ya no es necesario manejar el suelo aquí para reanudar velocidad, FixedUpdate lo gestiona.
        if (collidedLayer == groundLayer)
        {
            // El barril aterrizó. Solo reanuda velocidad, no invierte dirección.
            UpdateHorizontalVelocity();
            // A menos que quieras invertir en colisiones laterales con suelo normal, 
            // en cuyo caso llama a HandleGroundCollision(collidedObject, collision);
            return;
        }

        // 2. Manejar colisión con la pared inversora (con Bouncing)
        if (collidedLayer == inverterWallLayer)
        {
            HandleInverterWallCollision(collision);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HandlePlayerHit();
        }

        if (other.CompareTag("LadderDescend") && !isRollingDownLadder)
        {
            TryStartLadderDescent();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("LadderDescend") && isRollingDownLadder)
        {
            EndLadderDescent();
        }
    }

    // --- Métodos de Lógica de Colisión y Destrucción ---

    private bool HandleDestructiveCollisions(int layer, GameObject obj)
    {
        if (layer == killZoneLayer || layer == playerLayer)
        {
            if (layer == playerLayer)
            {
                // Asegúrate de que GameManager.Instance esté accesible si lo estás usando
                // if (GameManager.Instance != null) { GameManager.Instance.HandlePlayerHit(); }
                Debug.Log("Voltorb ha golpeado al jugador.");
            }
            Destroy(gameObject);
            return true;
        }
        return false;
    }

    private void HandlePlayerHit()
    {
        // Se asume que GameManager.Instance.HandlePlayerHit() existe
        // if (GameManager.Instance != null) { GameManager.Instance.HandlePlayerHit(); }
        Destroy(gameObject);
    }

    // ... (Mantén HandleGroundCollision y HandleInverterWallCollision si son necesarios para la inversión)

    private void HandleInverterWallCollision(Collision2D collision)
    {
        // Solo invertiremos la dirección si el golpe es claramente lateral.
        Vector2 normal = collision.contacts[0].normal;
        const float LATERAL_THRESHOLD = 0.3f; // Ajusta este valor

        if (Mathf.Abs(normal.y) < LATERAL_THRESHOLD)
        {
            // Choca con pared -> Invertir dirección
            horizontalDirection *= -1f;
        }

        // El rebote de Unity (bouncing) ya habrá aplicado un impulso, 
        // pero aseguramos la velocidad base.
        UpdateHorizontalVelocity();
    }


    private void UpdateHorizontalVelocity()
    {
        rb.linearVelocity = new Vector2(horizontalDirection * rollSpeed, rb.linearVelocity.y);
    }

    // --- Lógica de Escalera ---

    private void TryStartLadderDescent()
    {
        // Comprobar la probabilidad de bajar la escalera
        if (Random.value < ladderDescentChance)
        {
            isRollingDownLadder = true;

            // **IMPORTANTE para Audio:** Detener el sonido de rodadura inmediatamente
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            // Deshabilitar colisión con el suelo y detener el movimiento horizontal
            ToggleGroundCollisions(true);
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void EndLadderDescent()
    {
        isRollingDownLadder = false;

        // 1. FORZAR la inversión de la dirección horizontal al terminar el descenso
        horizontalDirection *= -1f;

        // 2. Reanudar Colisiones con el Suelo
        ToggleGroundCollisions(false);

        // 3. Reanudar el movimiento horizontal con la NUEVA dirección
        // El FixedUpdate gestionará la reanudación del sonido si aterriza en el suelo.
        UpdateHorizontalVelocity();
    }

    // ... (ToggleGroundCollisions se mantiene igual)

    private void ToggleGroundCollisions(bool ignore)
    {
        // Encontramos los colisionadores de suelo cercanos para ignorarlos o restaurarlos.
        Collider2D barrelCollider = GetComponent<Collider2D>();

        if (ignore)
        {
            ignoredGroundColliders.Clear();

            // Buscar colisionadores de suelo en un radio pequeño alrededor del barril.
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, OVERLAP_CHECK_RADIUS);

            foreach (Collider2D hitCollider in hitColliders)
            {
                if (hitCollider.gameObject.layer == groundLayer)
                {
                    Physics2D.IgnoreCollision(barrelCollider, hitCollider, true);
                    ignoredGroundColliders.Add(hitCollider);
                }
            }
        }
        else // Restaurar colisiones
        {
            foreach (Collider2D ignoredCollider in ignoredGroundColliders)
            {
                if (ignoredCollider != null)
                {
                    Physics2D.IgnoreCollision(barrelCollider, ignoredCollider, false);
                }
            }
            ignoredGroundColliders.Clear();
        }
    }
}