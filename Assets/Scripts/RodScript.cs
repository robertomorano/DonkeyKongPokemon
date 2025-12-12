using UnityEngine;

public class RodScript : MonoBehaviour
{
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

        // Configurar collider como trigger
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
        // Rotación
        if (rotate)
        {
            transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }

        // Rebote
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

                Debug.Log("¡Power-Up recogido!");

                // Destruir el objeto
                Destroy(gameObject);
            }
        }
    }
}
