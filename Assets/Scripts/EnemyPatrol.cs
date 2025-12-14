using UnityEngine;
using System.Collections; // Necesario para usar Corrutinas (IEnumerator)

public class EnemyPatrol : MonoBehaviour
{
    // --- Parámetros ajustables en el Inspector ---
    [Header("Movimiento")]
    public float moveSpeed = 2f;
    public float patrolDistance = 0.5f; // Distancia para verificar el borde

    [Header("Pausa")]
    public float pauseTime = 2f; // Tiempo que el enemigo se detiene en el borde

    [Header("Configuración de Detección")]
    public LayerMask groundLayer; // Capa que define la plataforma (el suelo)

    [Header("Audio")]
    public AudioSource audioSource;
    [Tooltip("Sonido constante que se repite mientras el enemigo camina.")]
    public AudioClip walkLoopClip;
    // La variable de volumen es necesaria, la reintroducimos aquí:
    [Range(0f, 1f)] public float soundVolume = 0.5f;

    // --- Componentes ---
    private Rigidbody2D rb;
    private bool isFacingRight = true;
    private bool isPatrolling = true; // Estado para controlar si debe moverse

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Asegurarse de que el Rigidbody2D no rote
        rb.freezeRotation = true;

        // >>> INICIALIZACIÓN DE AUDIO (CORREGIDA LA IDENTACIÓN) <<<
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        if (audioSource != null && walkLoopClip != null)
        {
            audioSource.clip = walkLoopClip;
            audioSource.loop = true;
            audioSource.volume = soundVolume;
            audioSource.Play(); // Inicia el sonido de movimiento constante
        }
        // >>> FIN INICIALIZACIÓN DE AUDIO <<<
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
        // Se usa 'velocity' en lugar de 'linearVelocity'
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
        // Se lanza ligeramente más adelante y debajo de la posición actual del enemigo.
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

        // Si hit.collider es null, significa que no encontró suelo -> está en un borde
        return hit.collider == null;
    }

    // Corrutina para pausar, girar y reanudar el patrullaje
    IEnumerator StopAndTurn()
    {
        isPatrolling = false; // Detener el movimiento en Update

        // >>> AUDIO: Detener el sonido de caminar (para la pausa) <<<
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // PAUSA: Esperar el tiempo especificado
        yield return new WaitForSeconds(pauseTime);

        // GIRAR: Cambiar la dirección y reflejar el sprite
        isFacingRight = !isFacingRight;
        Flip();

        // REANUDAR: Volver al estado de movimiento
        isPatrolling = true;

        // >>> AUDIO: Reanudar el sonido de caminar <<<
        if (audioSource != null && walkLoopClip != null)
        {
            audioSource.Play();
        }
    }

    // Función para girar visualmente el sprite del enemigo
    private void Flip()
    {
        // Invierte la escala local en el eje X para voltear el sprite
        Vector3 newScale = transform.localScale;
        newScale.x *= -1;
        transform.localScale = newScale;
    }
}