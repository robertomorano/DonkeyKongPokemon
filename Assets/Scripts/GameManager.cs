using UnityEngine;
using UnityEngine.SceneManagement;

// Asegúrate de incluir el namespace para la interfaz de usuario si usas Image:
// using UnityEngine.UI; 


public class GameManager : MonoBehaviour
{
    private string CurrentSceneName;
    public static GameManager Instance;

    public GameObject player;

    Transform initialPosition;
    public int lifes = 5; // Vida total. Es mejor si es un número que se divide bien por 3 o 2
                          // Se asume que 'lifes' representa la vida actual.

    // --- NUEVAS VARIABLES PARA EL HUD ---

    // Asignación de los Sprites de vida desde el Inspector
    public Sprite fullLifeSprite;
    public Sprite halfLifeSprite;
    public Sprite lowLifeSprite;

    // Referencia al componente SpriteRenderer del objeto HUD
    public SpriteRenderer hudSpriteRenderer;
    // Si usas UnityEngine.UI.Image, cambia el tipo a 'public Image hudImage;'

    // --- NUEVAS VARIABLES PARA EL HUD ---

    // La vida máxima para calcular los umbrales
    private int maxLifes;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initialPosition = player.transform;
        CurrentSceneName = SceneManager.GetActiveScene().name;

        // Guardar la vida máxima al inicio
        maxLifes = lifes;

        // Inicializar el HUD al estado de vida completo
        UpdateHUDVisual();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Reset()
    {
        SceneManager.LoadScene(CurrentSceneName);
        lifes = maxLifes; // Usar maxLifes para restablecer
    }

    // --- NUEVO MÉTODO PARA ACTUALIZAR EL SPRITE DEL HUD ---
    private void UpdateHUDVisual()
    {
        // Calcular los umbrales de vida (por ejemplo, Mitad = 66%, Low = 33%)
        // Asumiendo que la vida se divide en tercios para 3 estados:
        float lowThreshold = maxLifes / 3f;
        float halfThreshold = 2 * (maxLifes / 3f);

        // Ejemplo con 5 vidas: lowThreshold ≈ 1.66, halfThreshold ≈ 3.33

        Sprite newSprite = null;

        if (lifes > halfThreshold)
        {
            // Estado FULL (Más de 2/3 de vida)
            newSprite = fullLifeSprite;
        }
        else if (lifes > lowThreshold)
        {
            // Estado MITAD (Entre 1/3 y 2/3 de vida)
            newSprite = halfLifeSprite;
        }
        else
        {
            // Estado LOW (1/3 de vida o menos)
            newSprite = lowLifeSprite;
        }

        // Aplicar el nuevo sprite si la referencia al SpriteRenderer es válida
        if (hudSpriteRenderer != null && newSprite != null)
        {
            hudSpriteRenderer.sprite = newSprite;
        }
        // Si usas Image:
        // if (hudImage != null && newSprite != null)
        // {
        //     hudImage.sprite = newSprite;
        // }
    }
    // --- FIN DEL NUEVO MÉTODO ---


    public void HandlePlayerHit()
    {

        lifes--;

        // Llamar a la actualización del HUD inmediatamente después de reducir la vida
        UpdateHUDVisual();

        if (lifes > 0)
        {

            restartPlayer();
        }
        else
        {

            Reset();
        }
    }

    private void restartPlayer()
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // Corregido a 'velocity' en lugar de 'linearVelocity'
        }
        player.transform.position = initialPosition.position;
    }


}