using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    // Opcional: Para controlar cuánto daño hace el enemigo
    public int damageAmount = 1;

    // Se llama cuando el collider del enemigo (que no es Trigger) choca con otro collider
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. Verificar si el objeto con el que colisionamos es el jugador
        if (collision.gameObject.CompareTag("Player"))
        {
            // Opcional: Si quieres prevenir daño repetido inmediatamente
            // if (isInvulnerable) return; 

            // 2. Llamar a la función del GameManager usando la instancia estática
            // Es la forma más limpia de acceder al gestor de juego
            if (GameManager.Instance != null)
            {
                // Por ahora, solo llamamos a la función que resta 1 vida
                GameManager.Instance.HandlePlayerHit();

                // Nota: Tu función HandlePlayerHit ya gestiona la resta de vida,
                // por lo que no necesitamos usar 'damageAmount' a menos que
                // modifiques esa función para aceptar un parámetro de daño.
            }

            // 3. (Opcional) Puedes añadir lógica aquí para temporalmente
            // hacer al jugador invulnerable después del golpe (parpadeo, etc.)
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.HandlePlayerHit();
            }
        }
    }
   
}