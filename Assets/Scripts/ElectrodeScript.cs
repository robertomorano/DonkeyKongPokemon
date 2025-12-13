using UnityEngine;
using System.Collections.Generic;

public class ElectrodeScript : VoltorbScript
{
    [Header("Explosive Settings")]
    public float timeToExplode = 5f;
    public float explosionRadius = 2.0f;

    // Llamado cuando el script se activa.
    void Start()
    {
        // Usar la función Invoke para detonar después de un tiempo.
        Invoke(nameof(Explode), timeToExplode);
    }

    // Sobrescribir (override) la lógica de la colisión base:
    // Al chocar con el jugador, el barril explosivo detona inmediatamente.
    /*protected override void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. Ejecutar la lógica de colisión base (movimiento, inversión, etc.)
        base.OnCollisionEnter2D(collision);

        // 2. Lógica específica del barril explosivo:
        if (collision.gameObject.layer == playerLayer)
        {
            // El barril explosivo detona al golpear al jugador.
            CancelInvoke(nameof(Explode)); // Si el temporizador estaba activo
            Explode();
        }
    }*/

    private void Explode()
    {
        // Evitar que el código base siga moviendo o manipulando el barril.
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false; // Detiene la simulación de física
        }

        // --- LÓGICA DE DETECCIÓN DE DAÑO (Zona de Explosión) ---

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                // Manejar el daño al jugador
                GameManager.Instance.HandlePlayerHit();
            }
        }

        // Ejecutar efectos visuales y de audio de la explosión (ej. Instantiate(ExplosionPrefab)).

        Destroy(gameObject); // Eliminar el barril
    }
}