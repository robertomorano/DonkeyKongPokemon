using UnityEngine;

public class RodScript : MonoBehaviour
{
    // Duración del martillo una vez recogido
    [SerializeField] private float duration = 5f;

    private bool rotate = true;
    private float rotationSpeed = 100f;
    private bool bounce = true;
    private float bounceHeight = 0.03f;
    private float bounceSpeed = 2f;


    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;

        // Configurar collider como trigger para la recogida
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogWarning($"¡{gameObject.name} necesita un Collider2D con isTrigger activado!");
        }
    }

    void Update()
    {
        // Rotación visual
        if (rotate)
        {
            transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }

        // Rebote visual (usando la función Sinusoidal)
        if (bounce)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificar si es el jugador
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();

            if (player != null)
            {
                // ** Llama al método del jugador para activar el martillo **
                player.ActivateHammer(duration);

                Debug.Log("¡Martillo recogido! Duración: " + duration + "s");

                // Destruir el objeto (el ítem del martillo)
                Destroy(gameObject);
            }
        }
    }
}