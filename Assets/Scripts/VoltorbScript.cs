using UnityEngine;
using System.Collections.Generic;

public class VoltorbScript : MonoBehaviour
{
    // --- Configuraci�n P�blica ---
    [Header("Movement")]
    public float rollSpeed = 3f;
    [Range(0f, 1f)]
    public float ladderDescentChance = 0.4f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip rollingSoundClip;
    [Tooltip("Distancia hacia abajo para comprobar si est� tocando el suelo.")]
    public float groundCheckDistance = 0.6f; // Ajusta este valor seg�n el tama�o del colisionador

    // --- Componentes y Estado ---
    public Rigidbody2D rb;

    [Tooltip("Direcci�n horizontal actual de rodadura (-1: Izquierda, 1: Derecha)")]
    public float horizontalDirection = -1f; // Iniciar rodando hacia la izquierda
    private bool isRollingDownLadder = false;

    // Nuevo estado para la l�gica de sonido
    private bool isGrounded = false;

    // --- Capas y Constantes ---
    public int groundLayer;
    public int playerLayer;
    public int killZoneLayer;


    
    
    private int inverterWallLayer;
    private const float OVERLAP_CHECK_RADIUS = 0.5f;

    // Colisionadores de suelo ignorados durante el descenso por escalera
    private readonly List<Collider2D> ignoredGroundColliders = new List<Collider2D>();

    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        InitializeLayers();
        // Asegurarse de que el AudioSource est� configurado si no est� asignado en el Inspector
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                // Si no hay AudioSource, a�adir uno
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
            // No reproducir al inicio, se har� en FixedUpdate
        }
    }

    private void FixedUpdate()
    {
        // 1. Comprobar el estado de estar en el suelo
        CheckIfGrounded();

        // 2. Actualizar la reproducci�n del sonido de rodadura
        UpdateRollingSound();

        // Aseguramos que la velocidad horizontal sea correcta si no est� en descenso por escalera
        if (!isRollingDownLadder)
        {
            UpdateHorizontalVelocity();
        }
    }


    // --- L�gica de Suelo y Sonido ---

    private void CheckIfGrounded()
    {
        // Usar un Raycast o BoxCast peque�o hacia abajo para verificar si est� tocando el suelo.
        // Se usa el bitwise OR (|) para crear una m�scara de capa solo con la capa "Ground".
        int layerMask = 1 << groundLayer;

        // Se usa Physics2D.Raycast: empieza en el centro, va hacia abajo por 'groundCheckDistance'.
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, layerMask);

        // Si el raycast golpea algo en la capa de suelo, est� en el suelo.
        isGrounded = hit.collider != null;

        // Visualizaci�n del Raycast (solo en el editor)
        // Debug.DrawRay(transform.position, Vector2.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
    }

    private void UpdateRollingSound()
    {
        // Rodando si est� en el suelo Y movi�ndose horizontalmente, Y no est� descendiendo por la escalera (que detiene el movimiento horizontal)
        bool shouldBeRolling = isGrounded && Mathf.Abs(rb.linearVelocity.x) > 0.01f && !isRollingDownLadder;

        if (audioSource == null || rollingSoundClip == null)
        {
            return; // No hay audio para reproducir
        }

        if (shouldBeRolling && !audioSource.isPlaying)
        {
            // Deber�a rodar, pero el sonido est� detenido -> Empezar a rodar
            audioSource.Play();
        }
        else if (!shouldBeRolling && audioSource.isPlaying)
        {
            // No deber�a rodar, pero el sonido se est� reproduciendo -> Detener la rodadura
            audioSource.Stop();
        }
    }

    // --- M�todos de Colisi�n y Trigger (Mantener los m�todos existentes) ---
    // ... (El resto de los m�todos OnCollisionEnter2D, OnTriggerEnter2D, etc. se mantienen)


    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject collidedObject = collision.gameObject;
        int collidedLayer = collidedObject.layer;

        if (HandleDestructiveCollisions(collidedLayer, collidedObject))
        {
            return;
        }

        // 1. Manejar colisi�n con el suelo (Aterrizajes)
        // Ya no es necesario manejar el suelo aqu� para reanudar velocidad, FixedUpdate lo gestiona.
        if (collidedLayer == groundLayer)
        {
            // El barril aterriz�. Solo reanuda velocidad, no invierte direcci�n.
            UpdateHorizontalVelocity();
            // A menos que quieras invertir en colisiones laterales con suelo normal, 
            // en cuyo caso llama a HandleGroundCollision(collidedObject, collision);
            return;
        }

        // 2. Manejar colisi�n con la pared inversora (con Bouncing)
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

    // --- M�todos de L�gica de Colisi�n y Destrucci�n ---

    private bool HandleDestructiveCollisions(int layer, GameObject obj)
    {
        if (layer == killZoneLayer || layer == playerLayer)
        {
            if (layer == playerLayer)
            {
                // Aseg�rate de que GameManager.Instance est� accesible si lo est�s usando
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

    // ... (Mant�n HandleGroundCollision y HandleInverterWallCollision si son necesarios para la inversi�n)

    private void HandleInverterWallCollision(Collision2D collision)
    {
        // Solo invertiremos la direcci�n si el golpe es claramente lateral.
        Vector2 normal = collision.contacts[0].normal;
        const float LATERAL_THRESHOLD = 0.3f; // Ajusta este valor

        if (Mathf.Abs(normal.y) < LATERAL_THRESHOLD)
        {
            // Choca con pared -> Invertir direcci�n
            horizontalDirection *= -1f;
        }

        // El rebote de Unity (bouncing) ya habr� aplicado un impulso, 
        // pero aseguramos la velocidad base.
        UpdateHorizontalVelocity();
    }


    private void UpdateHorizontalVelocity()
    {
        rb.linearVelocity = new Vector2(horizontalDirection * rollSpeed, rb.linearVelocity.y);
    }

    // --- L�gica de Escalera ---

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

            // Deshabilitar colisi�n con el suelo y detener el movimiento horizontal
            ToggleGroundCollisions(true);
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void EndLadderDescent()
    {
        isRollingDownLadder = false;

        // 1. FORZAR la inversi�n de la direcci�n horizontal al terminar el descenso
        horizontalDirection *= -1f;

        // 2. Reanudar Colisiones con el Suelo
        ToggleGroundCollisions(false);

        // 3. Reanudar el movimiento horizontal con la NUEVA direcci�n
        // El FixedUpdate gestionar� la reanudaci�n del sonido si aterriza en el suelo.
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

            // Buscar colisionadores de suelo en un radio peque�o alrededor del barril.
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