using UnityEngine;

public class BarrelJumping : MonoBehaviour
{
    [Header("Configuraci�n de Movimiento")]
    public float velocidadHorizontal = 5f;
    public float fuerzaSalto = 8f;
    public float intervaloSalto = 2.5f;

    [Header("Configuraci�n de Colisi�n")]
    public LayerMask capaSuelo; // Asigna las capas de suelo/plataforma en el Inspector

    // --- Variables de Estado Interno ---
    private Rigidbody2D rb;
    private float direccionHorizontal = 1f; // 1 = derecha, -1 = izquierda
    private float tiempoHastaSiguienteSalto;
    private bool estaEnSuelo = false;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Inicializa el temporizador de salto en el primer intervalo
        tiempoHastaSiguienteSalto = intervaloSalto;
    }

    void Update()
    {
        // 1. Contador de tiempo para el pr�ximo salto
        tiempoHastaSiguienteSalto -= Time.deltaTime;

        // 2. Comprobar si es hora de saltar y si est� en el suelo
        if (tiempoHastaSiguienteSalto <= 0f && estaEnSuelo)
        {
            Saltar();
        }
    }

    void FixedUpdate()
    {
        // 3. Aplicar la velocidad horizontal solo si no est� cayendo por gravedad (m�s limpio)
        if (estaEnSuelo)
        {
            ActualizarVelocidadHorizontal();
        }
        // Si est� en el aire, solo aplicamos la gravedad y mantenemos el impulso actual.
    }

    private void ActualizarVelocidadHorizontal()
    {
        // Mantiene la velocidad horizontal constante
        rb.linearVelocity = new Vector2(direccionHorizontal * velocidadHorizontal, rb.linearVelocity.y);
    }

    private void Saltar()
    {
        // 1. Aplica la fuerza de salto vertical
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // Resetea velocidad Y antes de saltar
        rb.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);

        // 2. Reinicia el temporizador
        tiempoHastaSiguienteSalto = intervaloSalto;
        estaEnSuelo = false; // El barril ya no est� en el suelo
    }


    // --- L�GICA DE COLISI�N ---

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Comprueba si la colisi�n es con el suelo/plataforma (capa permitida)
        if (((1 << collision.gameObject.layer) & capaSuelo) != 0)
        {
            // 1. Detecci�n de suelo para futuros saltos
            // Comprobamos la normal de colisi�n para distinguir el aterrizaje (Y=1) de la pared (X=1)
            Vector2 normal = collision.contacts[0].normal;

            // Si la colisi�n es predominantemente vertical (es un aterrizaje)
            if (normal.y > 0.7f) // Umbral de 0.7f para asegurar que es un suelo
            {
                estaEnSuelo = true;
                ActualizarVelocidadHorizontal(); // Reanuda el movimiento horizontal al aterrizar
            }
            else // Es una pared (colisi�n lateral)
            {
                ManejarColisionPared();
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // Si sale de la colisi�n con la capa de suelo, asume que est� en el aire.
        if (((1 << collision.gameObject.layer) & capaSuelo) != 0)
        {
            // Para evitar reseteo accidental con roces, puedes a�adir un peque�o retraso
            // o un raycast, pero para la l�gica b�sica, esto es suficiente.
            estaEnSuelo = false;
        }
    }

    private void ManejarColisionPared()
    {
        // Invierte la direcci�n horizontal
        direccionHorizontal *= -1f;
        ActualizarVelocidadHorizontal();
    }
}