using UnityEngine;

public class PointItem : MonoBehaviour
{
    public int points = 500;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // ACCESO EFICIENTE con Singleton:
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(points);
            }

            // destruir el ítem
            Destroy(gameObject);
        }
    }
}