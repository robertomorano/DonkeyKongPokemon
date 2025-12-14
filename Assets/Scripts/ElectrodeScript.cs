using UnityEngine;
using System.Collections.Generic;

// Renombramos la clase para que refleje su propósito (Explosive Barrel)
// Asumimos que VoltorbScript es la clase base que contiene Rigidbody2D (rb)
// y las variables de capa (playerLayer, etc.).
public class ElectrodeScript : VoltorbScript
{
    [Header("Explosive Settings")]
    public float timeToExplode = 5f;
    public float explosionRadius = 2.0f;

    // Componente Animator
    private Animator anim;

    // Nombre del trigger que activará la animación de explosión
    private const string EXPLODE_TRIGGER = "Detonate";

    // El método Awake es llamado internamente por Unity y no necesita ser public.
    // Usamos 'private' y llamamos al base.Awake() si es necesario.
    private void Awake()
    {
        // Si VoltorbScript hereda de MonoBehaviour, no tiene Awake virtual por defecto.
        // Asumiendo que VoltorbScript maneja la inicialización de rb.
        // base.Awake() no es necesario a menos que VoltorbScript lo haga virtual.

        // Inicialización de la clase hija
        anim = GetComponent<Animator>();

        // Llamada a la inicialización de la base (asumiendo que inicializa rb, etc.)
        // Si VoltorbScript es la clase BarrelController que refactorizamos, rb es protected.
        // Si no se puede llamar a base.Awake(), puedes mover la inicialización de rb aquí si fuera necesario.
        base.Awake();
    }

    void Start()
    {
        // Usar la función Invoke para detonar después de un tiempo.
        Invoke(nameof(PrepareExplosion), timeToExplode);
    }

    // Creamos un método intermedio para detener el movimiento antes de la animación
    private void PrepareExplosion()
    {
        // 1. Detiene el movimiento inmediatamente antes de detonar
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // Usar 'velocity' en lugar de 'linearVelocity' si es posible
            rb.simulated = false; // Desactiva la física
        }

        // 2. Ejecuta la detección de daño inmediatamente (la onda expansiva)
        CheckForDamage();

        // 3. Activa el Trigger de la animación
        if (anim != null)
        {
            anim.SetTrigger(EXPLODE_TRIGGER);
            
        }

        // La destrucción se manejará mediante el Animation Event (FinishExplosion()).
    }

    // Método para manejar colisiones, sobrescribe el método virtual de la clase base.
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        // **Lógica Corregida:**
        // Solo procesamos colisiones si el barril aún NO ha iniciado la cuenta regresiva de la explosión.
        // Si IsInvoking(nameof(PrepareExplosion)) es TRUE, significa que está activo y rodando.
        // Si IsInvoking() es FALSE, significa que ya explotó o ya se está preparando para explotar.
        if (IsInvoking(nameof(PrepareExplosion)))
        {
            // 1. Ejecutar la lógica de colisión base (movimiento, inversión, etc.)
            // Esto requiere que OnCollisionEnter2D en VoltorbScript sea 'protected virtual'.
            base.OnCollisionEnter2D(collision);

            // 2. Lógica de detonación inmediata al golpear al jugador
            if (collision.gameObject.layer == playerLayer)
            {
                CancelInvoke(nameof(PrepareExplosion));
                PrepareExplosion();
            }
        }
        // Si ya está explotando, no hace nada (no vuelve a llamar a la base).
    }

    // Método que maneja la lógica de daño/radio
    private void CheckForDamage()
    {
        // --- LÓGICA DE DETECCIÓN DE DAÑO (Zona de Explosión) ---
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                // Solo inflige daño una vez
                GameManager.Instance.HandlePlayerHit();
                // Si tienes otros objetos que puedan ser destruidos, añádelos aquí
            }
        }
    }

    // Método público que DEBE ser llamado por el Evento de Animación
    public void FinishExplosion()
    {
        // 3. Destrucción final del objeto
        Destroy(gameObject);
    }
}