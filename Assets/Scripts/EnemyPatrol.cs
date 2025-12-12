using UnityEngine;
using System.Collections; // Necesario para usar Corrutinas (IEnumerator)

public class EnemyPatrol : MonoBehaviour
{
    // --- Par�metros ajustables en el Inspector ---
    [Header("Movimiento")]
    public float moveSpeed = 2f;
    public float patrolDistance = 0.5f; // Distancia para verificar el borde

    [Header("Pausa")]
    public float pauseTime = 2f; // Tiempo que el enemigo se detiene en el borde

    [Header("Configuraci�n de Detecci�n")]
    public LayerMask groundLayer; // Capa que define la plataforma (el suelo)

    // --- Componentes ---
    private Rigidbody2D rb;
    private bool isFacingRight = true;
    private bool isPatrolling = true; // Estado para controlar si debe moverse

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Asegurarse de que el Rigidbody2D no rote
        rb.freezeRotation = true;
    }

    void Update()
    {
        if (isPatrolling)
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        // 1. Mover al enemigo
        // Si isFacingRight es true, se mueve a la derecha (1). Si es false, a la izquierda (-1).
        float direction = isFacingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        // 2. Detectar el borde de la plataforma
        if (IsAtEdge())
        {
            // Detener el movimiento e iniciar la pausa
            rb.linearVelocity = Vector2.zero;
            StartCoroutine(StopAndTurn());
        }
    }

    private bool IsAtEdge()
    {
        // Calcular el punto de origen del Raycast
        // Se lanza ligeramente m�s adelante y debajo de la posici�n actual del enemigo.
        Vector3 originOffset = new Vector3(
            (isFacingRight ? patrolDistance : -patrolDistance), // Desplazamiento horizontal
            -0.1f, // Ligeramente debajo del centro para verificar si hay suelo
            0
        );

        Vector2 raycastOrigin = transform.position + originOffset;

        // Lanzar el Raycast hacia abajo
        // Detecta si NO hay suelo inmediatamente por delante y debajo
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.down, 1f, groundLayer);

        // Opcional: Para ver el Raycast en el editor (solo para debugging)
        Color rayColor = hit.collider == null ? Color.red : Color.green;
        Debug.DrawRay(raycastOrigin, Vector2.down * 1f, rayColor);

        // Si hit.collider es null, significa que no encontr� suelo -> est� en un borde
        return hit.collider == null;
    }

    // Corrutina para pausar, girar y reanudar el patrullaje
    IEnumerator StopAndTurn()
    {
        isPatrolling = false; // Detener el movimiento en Update

        // PAUSA: Esperar el tiempo especificado
        yield return new WaitForSeconds(pauseTime);

        // GIRAR: Cambiar la direcci�n y reflejar el sprite
        isFacingRight = !isFacingRight;
        Flip();

        // REANUDAR: Volver al estado de movimiento
        isPatrolling = true;
    }

    // Funci�n para girar visualmente el sprite del enemigo
    private void Flip()
    {
        // Invierte la escala local en el eje X para voltear el sprite
        Vector3 newScale = transform.localScale;
        newScale.x *= -1;
        transform.localScale = newScale;
    }
}