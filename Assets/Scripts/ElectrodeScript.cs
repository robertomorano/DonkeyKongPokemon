using UnityEngine;
using System.Collections.Generic;

public class ElectrodeScript : VoltorbScript
{
    [Header("Explosive Settings")]
    public float timeToExplode = 5f;
    public float explosionRadius = 2.0f;

    [Header("Explosion Audio")]
    public AudioClip explosionSound;
    // NUEVO: AudioSource dedicado a la explosión
    public AudioSource audioSourceExplosion;

    private Animator anim;
    private const string EXPLODE_TRIGGER = "Detonate";

    protected override void Awake()
    {
        // Llama a la inicialización de VoltorbScript (rb, audioSourceRolling)
        base.Awake();

        anim = GetComponent<Animator>();

        // Comprobación de que ambos AudioSources estén asignados o se encuentren
        if (audioSourceRolling == null)
        {
            Debug.LogWarning("AudioSource for rolling is missing on Electrode.");
        }
        if (audioSourceExplosion == null)
        {
            // Opcional: Intentar buscar el segundo AudioSource si no está asignado
            Debug.LogWarning("AudioSource for explosion is missing on Electrode. Please assign the second AudioSource.");
        }
    }

    protected override void Start()
    {
        base.Start();
        Invoke(nameof(PrepareExplosion), timeToExplode);
    }

    private void PrepareExplosion()
    {
        // 1. Detiene el movimiento
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // 2. Detiene SOLO el sonido de rodadura usando la variable de la base
        if (audioSourceRolling != null)
        {
            audioSourceRolling.Stop();
        }

        // 3. Reproduce la explosión usando el AudioSource dedicado
        if (audioSourceExplosion != null && explosionSound != null)
        {
            audioSourceExplosion.PlayOneShot(explosionSound);
            Debug.Log("¡Sonido de explosión reproducido en source dedicado!");
        }

        // 4. Ejecuta la detección de daño y animación
        CheckForDamage();
        if (anim != null)
        {
            anim.SetTrigger(EXPLODE_TRIGGER);
        }
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsInvoking(nameof(PrepareExplosion)))
        {
            base.OnCollisionEnter2D(collision);

            if (collision.gameObject.layer == playerLayer)
            {
                CancelInvoke(nameof(PrepareExplosion));
                PrepareExplosion();
            }
        }
    }

    private void CheckForDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.HandlePlayerHit();
                }
            }
        }
    }

    public void FinishExplosion()
    {
        Destroy(gameObject);
    }
}