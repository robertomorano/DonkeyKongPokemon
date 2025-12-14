using UnityEngine;
using System.Collections.Generic;

public class VoltorbScript : MonoBehaviour
{
    // --- Configuración Pública ---
    [Header("Movement")]
    public float rollSpeed = 3f;
    [Range(0f, 1f)]
    public float ladderDescentChance = 0.4f;

    [Header("Audio (Rolling)")]
    // Cambiamos el nombre para que el Electrode use el suyo propio
    public AudioSource audioSourceRolling;
    public AudioClip rollingSoundClip;
    [Tooltip("Distancia hacia abajo para comprobar si está tocando el suelo.")]
    public float groundCheckDistance = 0.6f;

    // --- Componentes y Estado ---
    protected Rigidbody2D rb;

    [Tooltip("Dirección horizontal actual de rodadura (-1: Izquierda, 1: Derecha)")]
    public float horizontalDirection = -1f;
    protected bool isRollingDownLadder = false;

    protected bool isGrounded = false;

    // --- Capas y Constantes ---
    protected int groundLayer;
    protected int playerLayer;
    protected int killZoneLayer;
    protected int inverterWallLayer;
    private const float OVERLAP_CHECK_RADIUS = 0.5f;

    private readonly List<Collider2D> ignoredGroundColliders = new List<Collider2D>();

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        InitializeLayers();

        // Inicialización del AudioSource de rodadura
        if (audioSourceRolling == null)
        {
            // Intentamos obtener el primero si no está asignado
            audioSourceRolling = GetComponent<AudioSource>();
        }

        if (audioSourceRolling == null)
        {
            Debug.LogWarning("AudioSource for rolling is missing on Voltorb/Electrode.");
        }
    }

    private void InitializeLayers()
    {
        groundLayer = LayerMask.NameToLayer("Ground");
        playerLayer = LayerMask.NameToLayer("Player");
        killZoneLayer = LayerMask.NameToLayer("KillZone");
        inverterWallLayer = LayerMask.NameToLayer("Wall");
    }

    protected virtual void Start()
    {
        UpdateHorizontalVelocity();

        // Configurar el AudioSource para el sonido de rodadura
        if (audioSourceRolling != null && rollingSoundClip != null)
        {
            audioSourceRolling.clip = rollingSoundClip;
            audioSourceRolling.loop = true;
        }
    }

    private void FixedUpdate()
    {
        CheckIfGrounded();
        UpdateRollingSound();

        if (!isRollingDownLadder)
        {
            UpdateHorizontalVelocity();
        }
    }

    // --- Lógica de Suelo y Sonido ---

    private void CheckIfGrounded()
    {
        int layerMask = 1 << groundLayer;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, layerMask);
        isGrounded = hit.collider != null;
    }

    private void UpdateRollingSound()
    {
        bool shouldBeRolling = isGrounded && Mathf.Abs(rb.linearVelocity.x) > 0.01f && !isRollingDownLadder;

        if (audioSourceRolling == null || rollingSoundClip == null)
        {
            return;
        }

        if (shouldBeRolling && !audioSourceRolling.isPlaying)
        {
            audioSourceRolling.Play();
        }
        else if (!shouldBeRolling && audioSourceRolling.isPlaying)
        {
            audioSourceRolling.Stop();
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject collidedObject = collision.gameObject;
        int collidedLayer = collidedObject.layer;

        if (HandleDestructiveCollisions(collidedLayer, collidedObject))
        {
            return;
        }

        if (collidedLayer == groundLayer)
        {
            UpdateHorizontalVelocity();
            return;
        }

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

    private bool HandleDestructiveCollisions(int layer, GameObject obj)
    {
        if (layer == killZoneLayer || layer == playerLayer)
        {
            Destroy(gameObject);
            return true;
        }
        return false;
    }

    private void HandlePlayerHit()
    {
        Destroy(gameObject);
    }

    private void HandleInverterWallCollision(Collision2D collision)
    {
        Vector2 normal = collision.contacts[0].normal;
        const float LATERAL_THRESHOLD = 0.3f;

        if (Mathf.Abs(normal.y) < LATERAL_THRESHOLD)
        {
            horizontalDirection *= -1f;
        }
        UpdateHorizontalVelocity();
    }


    protected void UpdateHorizontalVelocity()
    {
        rb.linearVelocity = new Vector2(horizontalDirection * rollSpeed, rb.linearVelocity.y);
    }

    // --- Lógica de Escalera ---

    private void TryStartLadderDescent()
    {
        if (Random.value < ladderDescentChance)
        {
            isRollingDownLadder = true;

            if (audioSourceRolling != null && audioSourceRolling.isPlaying)
            {
                audioSourceRolling.Stop();
            }

            ToggleGroundCollisions(true);
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void EndLadderDescent()
    {
        isRollingDownLadder = false;
        horizontalDirection *= -1f;
        ToggleGroundCollisions(false);
        UpdateHorizontalVelocity();
    }

    private void ToggleGroundCollisions(bool ignore)
    {
        Collider2D barrelCollider = GetComponent<Collider2D>();

        if (ignore)
        {
            ignoredGroundColliders.Clear();
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
        else
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