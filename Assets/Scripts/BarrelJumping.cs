using UnityEngine;

public class BarrelJumping : MonoBehaviour
{
    // --- Configuración de Movimiento ---
    [Header("Configuración de Movimiento")]
    [Tooltip("Velocidad horizontal constante del barril.")]
    public float velocidadHorizontal = 1f;
    [Tooltip("Fuerza del impulso vertical al saltar.")]
    public float fuerzaSalto = 4f;

    // --- Configuración de Audio ---
    [Header("Configuración de Audio")]
    public AudioSource audioSource;
    public AudioClip rollingSoundClip;
    [Range(0f, 1f)] public float rollingVolume = 0.6f;

    // --- Configuración de Colisión ---
    [Header("Configuración de Colisión")]
    [Tooltip("Máscara de capas que definen el suelo o plataformas válidas.")]
    public LayerMask capaSuelo;
    [Tooltip("Capa que elimina al barril.")]
    public LayerMask capaKillZone;
    [Tooltip("Capa del Jugador.")]
    public LayerMask capaPlayer;

    // --- Variables de Componentes y Estado ---
    private Rigidbody2D rb;
    private float direccionHorizontal = 1f; // 1 = derecha, -1 = izquierda
    private bool puedeSaltar = false;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on Jumping Barrel.");
        }

        // >>> LÓGICA DE INICIALIZACIÓN DE AUDIO <<<
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                // Si no existe, lo añade
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        if (audioSource != null && rollingSoundClip != null)
        {
            audioSource.clip = rollingSoundClip;
            audioSource.loop = true;
            audioSource.volume = rollingVolume;
            audioSource.Play(); // Comienza el sonido inmediatamente
        }
        // >>> FIN LÓGICA DE AUDIO <<<


        // Asegurar que el barril comienza con el movimiento horizontal.
        ActualizarVelocidadHorizontal();
    }

    void FixedUpdate()
    {
        ActualizarVelocidadHorizontal();
        ActualizarOrientacionSprite();
    }

    private void ActualizarVelocidadHorizontal()
    {
        // Se ha cambiado a 'velocity' por consistencia con otros scripts de Unity
        rb.linearVelocity = new Vector2(direccionHorizontal * velocidadHorizontal, rb.linearVelocity.y);
    }

    private void Saltar()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);

        puedeSaltar = false;
    }

    private void ActualizarOrientacionSprite()
    {
        if (spriteRenderer == null) return;

        if (direccionHorizontal > 0.01f)
        {
            spriteRenderer.flipX = false;
        }
        else if (direccionHorizontal < -0.01f)
        {
            spriteRenderer.flipX = true;
        }
    }

    private void ManejarColisionPared()
    {
        direccionHorizontal *= -1f;
        ActualizarVelocidadHorizontal();
    }

    private void ManejarDestruccion(GameObject collidedObject)
    {
        // >>> LÓGICA PARA DETENER EL SONIDO AL MORIR <<<
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        // >>> FIN LÓGICA PARA DETENER EL SONIDO AL MORIR <<<

        if (collidedObject.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.HandlePlayerHit();
            }
        }
        Destroy(gameObject);
    }


    // --- LÓGICA DE COLISIÓN ---

    void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject collidedObject = collision.gameObject;
        int layer = collidedObject.layer;

        // 1. Detección de Destrucción (KillZone o Player)
        if (((1 << layer) & capaKillZone) != 0 || ((1 << layer) & capaPlayer) != 0)
        {
            ManejarDestruccion(collidedObject);
            return;
        }

        // 2. Detección de Suelo y Pared
        if (((1 << layer) & capaSuelo) != 0)
        {
            Vector2 normal = collision.contacts[0].normal;

            if (normal.y > 0.7f) // Aterrizaje
            {
                if (!puedeSaltar)
                {
                    puedeSaltar = true;
                    Saltar();
                }
            }
            else // Pared (Colisión lateral)
            {
                ManejarColisionPared();
            }
        }
    }
}