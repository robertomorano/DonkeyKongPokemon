using UnityEngine;
using System.Collections.Generic;

public class VoltorbScript : MonoBehaviour
{
    // --- Configuración Pública ---
    [Header("Movement")]
    public float rollSpeed = 3f;
    [Range(0f, 1f)]
    public float ladderDescentChance = 0.4f;

    // --- Componentes y Estado ---
    public Rigidbody2D rb;

    [Tooltip("Dirección horizontal actual de rodadura (-1: Izquierda, 1: Derecha)")]
    public float horizontalDirection = -1f; // Iniciar rodando hacia la izquierda
    private bool isRollingDownLadder = false;

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
    }

    private void InitializeLayers()
    {
        groundLayer = LayerMask.NameToLayer("Ground");
        playerLayer = LayerMask.NameToLayer("Player");
        killZoneLayer = LayerMask.NameToLayer("KillZone");
        inverterWallLayer = LayerMask.NameToLayer("Wall");
        

    }

    private void Start()
    {
        // Establecer la velocidad inicial de rodadura
        rb.linearVelocity = new Vector2(horizontalDirection * rollSpeed, 0f);
    }



    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject collidedObject = collision.gameObject;
        int collidedLayer = collidedObject.layer;

        if (HandleDestructiveCollisions(collidedLayer, collidedObject))
        {
            return;
        }

        // 1. Manejar colisión con el suelo (Aterrizajes)
        if (collidedLayer == groundLayer)
        {
            // El barril aterrizó. Solo reanuda velocidad, no invierte dirección.
            UpdateHorizontalVelocity();
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
                GameManager.Instance.HandlePlayerHit();
            }
            Destroy(gameObject);
            return true;
        }
        return false;
    }

    private void HandlePlayerHit()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.HandlePlayerHit();
        }
        Destroy(gameObject);
    }

    private void HandleGroundCollision(GameObject collidedObject, Collision2D collision)
    {
        // 1. Obtener la normal de colisión.
        // La normal es un vector perpendicular a la superficie de contacto.
        Vector2 normal = collision.contacts[0].normal;
        const float LATERAL_THRESHOLD = 0.3f;
        // Si choca con un objeto con Tags que no deben invertir el movimiento
        if (collidedObject.CompareTag("Finish") || collidedObject.CompareTag("Respawn"))
        {
            UpdateHorizontalVelocity();
            return;
        }

        // 2. Comprobar si la colisión es lateral (choque de pared)
        // Usamos un umbral (ej. 0.2f) para determinar si la colisión es horizontal.
        // Si la componente Y de la normal es pequeña, la colisión es lateral.
        // Mathf.Abs(normal.y) < 0.2f significa que la superficie de contacto es casi vertical.
        if (Mathf.Abs(normal.y) < LATERAL_THRESHOLD)
        {
            // Choca con pared normal (Ground) -> Invertir dirección
            horizontalDirection *= -1f;
            UpdateHorizontalVelocity();
        }
        // Si no es una colisión lateral (es un aterrizaje), simplemente reanudamos la velocidad
        // en caso de que la fricción la haya disminuido.
        else
        {
            UpdateHorizontalVelocity();
        }
    }

    private void HandleInverterWallCollision(Collision2D collision)
    {
        // Solo invertiremos la dirección si el golpe es claramente lateral.
        // Esto evita que el rebote sutil en un borde active la inversión.
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
        UpdateHorizontalVelocity();
    }

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


