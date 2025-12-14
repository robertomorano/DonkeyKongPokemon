using UnityEngine;

public class BarrelJumping : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [Tooltip("Velocidad horizontal constante del barril.")]
    public float velocidadHorizontal = 1f;
    [Tooltip("Fuerza del impulso vertical al saltar.")]
    public float fuerzaSalto = 4f;

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

    // Para la rotación del sprite
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on Jumping Barrel.");
        }

        // Asegurar que el barril comienza con el movimiento horizontal.
        ActualizarVelocidadHorizontal();
    }

    void FixedUpdate()
    {
        ActualizarVelocidadHorizontal();
        // Llama a la lógica de orientación del sprite en cada frame
        ActualizarOrientacionSprite();
    }

    private void ActualizarVelocidadHorizontal()
    {
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
        // Si la dirección horizontal es positiva (moviéndose a la derecha)
        if (direccionHorizontal > 0.01f)
        {
            // No voltear (normalmente false o 0 grados)
            spriteRenderer.flipX = false;
        }
        // Si la dirección horizontal es negativa (moviéndose a la izquierda)
        else if (direccionHorizontal < -0.01f)
        {
            // Voltear horizontalmente
            spriteRenderer.flipX = true;
        }
    }

    private void ManejarColisionPared()
    {
        // Invierte la dirección horizontal
        direccionHorizontal *= -1f;
        ActualizarVelocidadHorizontal();
    }

    private void ManejarDestruccion(GameObject collidedObject)
    {
        if (collidedObject.CompareTag("Player"))
        {
            // Si golpea al jugador, manejamos el daño antes de la destrucción
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