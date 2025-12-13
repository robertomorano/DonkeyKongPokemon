using UnityEngine;

public class ElectrodeScript : MonoBehaviour
{
    private Rigidbody2D rb;

    
    public float speed = 3f;
    
    public float ladderDescentChance = 0.4f; // 40% de probabilidad de bajar

    // Variables de control
    private bool isRollingDownLadder = false;

    // Variable NUEVA: Almacena la dirección horizontal de rodadura (-1 o 1)
    private float horizontalDirection = -1f;

    // Almacenamos el número de Layer del suelo para ignorarlo
    private int groundLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Obtener el ID de la capa "Ground" (debe existir en el editor de Unity)
        groundLayer = LayerMask.NameToLayer("Ground");
    }

    private void Start()
    {
        // Establecer la velocidad inicial usando la dirección guardada
        //rb.linearVelocity = new Vector2(horizontalDirection * speed, 0f);
    }

    
    private void FixedUpdate()
    {
       /* if (!isRollingDownLadder)
        {
            // Actualizar la dirección horizontal si la velocidad actual es muy baja 
            // y no estamos rodando por una escalera.
            if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
            {
                // Guarda la dirección actual de rodadura
                horizontalDirection = Mathf.Sign(rb.linearVelocity.x);
            }

            // Si el barril ha dejado de rodar por fricción (aunque debería rebotar siempre)
            if (Mathf.Abs(rb.linearVelocity.x) < 0.1f)
            {
                // Forzar la reanudación del movimiento
                rb.linearVelocity = new Vector2(horizontalDirection * speed, rb.linearVelocity.y);
            }
        }*/
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. Colisiones de Fin de Juego (mantener)
        if (collision.gameObject.layer == LayerMask.NameToLayer("KillZone") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                GameManager.Instance.HandlePlayerHit();
            }
            Destroy(gameObject);
            
        }

        // 2. Lógica de inversión de dirección al golpear una pared (si el rebote no es 100%)
        if (collision.gameObject.layer == groundLayer)
        {
            // Invertimos la dirección de rodadura
            horizontalDirection *= -1f;
            // Forzamos la nueva velocidad para que el PhysicsMaterial no la frene
           // rb.linearVelocity = new Vector2(horizontalDirection * speed, rb.linearVelocity.y);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.HandlePlayerHit();
            Destroy(gameObject);
            
        }

        // Lógica de Descenso por Escalera
        if (other.CompareTag("Ladder") && !isRollingDownLadder)
        {
            if (Random.value < ladderDescentChance)
            {
                isRollingDownLadder = true;

                // **PROBLEMA 2 SOLUCIONADO: Ignorar colisiones con el suelo**
                // Hacemos que el Rigidbody del barril ignore temporalmente su propia capa
                // con la capa Ground para que caiga por el hueco.
                Physics2D.IgnoreLayerCollision(gameObject.layer, groundLayer, true);

                // Centrar y detener movimiento horizontal (dejar que la gravedad haga el resto)
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Detectar si el barril ha pasado la escalera y ha caído en la siguiente plataforma.
        if (other.CompareTag("Ladder") && isRollingDownLadder)
        {
            // El barril ha salido de la zona del Trigger.
            isRollingDownLadder = false;

            // **PROBLEMA 1 SOLUCIONADO: Reanudar la dirección anterior**
            // Reanudar el movimiento horizontal usando la dirección recordada (horizontalDirection)
            rb.linearVelocity = new Vector2(horizontalDirection * speed, rb.linearVelocity.y);

            // **PROBLEMA 2 SOLUCIONADO: Restaurar colisiones con el suelo**
            // Volver a habilitar la colisión con la capa Ground.
            Physics2D.IgnoreLayerCollision(gameObject.layer, groundLayer, false);
        }
    }
}