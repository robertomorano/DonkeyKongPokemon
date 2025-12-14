using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private string CurrentSceneName;
    public static GameManager Instance;

    [Header("Referencias de Juego")]
    public GameObject player;
    public GameObject startPosition;
    public GameObject gameOverUI;

    [Header("Audio de Game Over")]
    public AudioSource gameOverAudioSource;
    public AudioClip gameOverSoundClip;
    [Range(0f, 1f)] public float gameOverVolume = 1.0f;

    [Header("HUD")]
    public Sprite fullLifeSprite;
    public Sprite halfLifeSprite;
    public Sprite lowLifeSprite;
    public SpriteRenderer hudSpriteRenderer;

    // --- Variables de Estado ---
    private bool isDead = false;
    public int lifes = 3;
    private int maxLifes;

    void Awake()
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

    void Start()
    {
        // Inicialización de AudioSource para Game Over
        if (gameOverAudioSource == null)
        {
            gameOverAudioSource = GetComponent<AudioSource>();
            if (gameOverAudioSource == null)
            {
                gameOverAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Aseguramos que el audioSource de Game Over no se buclee.
        if (gameOverAudioSource != null)
        {
            gameOverAudioSource.loop = false;
            gameOverAudioSource.playOnAwake = false;
        }

        if (player != null && startPosition != null)
        {
            player.transform.position = startPosition.transform.position;
        }

        CurrentSceneName = SceneManager.GetActiveScene().name;
        maxLifes = lifes;

        UpdateHUDVisual();

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }

        // Restaurar volumen al inicio del juego
        AudioListener.volume = 1f;
        Time.timeScale = 1f; // Asegurar que el tiempo esté corriendo al inicio
    }

    public void LoadCurrentScene()
    {
        // Si hay alguna invocación pendiente de Game Over, la cancelamos
        CancelInvoke(nameof(ShowGameOverUI));

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }

        lifes = maxLifes;
        isDead = false;

        // Restaurar volumen y tiempo
        AudioListener.volume = 1f;
        Time.timeScale = 1f;

        SceneManager.LoadScene(CurrentSceneName);
    }

    // (El resto de los métodos de HUD y manejo de golpes se mantienen iguales)

    private void UpdateHUDVisual()
    {
        float lowThreshold = maxLifes / 3f;
        float halfThreshold = 2 * (maxLifes / 3f);

        Sprite newSprite = null;

        if (lifes > halfThreshold)
        {
            newSprite = fullLifeSprite;
        }
        else if (lifes > lowThreshold)
        {
            newSprite = halfLifeSprite;
        }
        else
        {
            newSprite = lowLifeSprite;
        }

        if (hudSpriteRenderer != null && newSprite != null)
        {
            hudSpriteRenderer.sprite = newSprite;
        }
    }

    public void HandlePlayerHit()
    {
        if (isDead) return;

        lifes--;

        UpdateHUDVisual();

        if (lifes > 0)
        {
            restartPlayer();
        }
        else
        {
            isDead = true;
            GameOver();
        }
    }

    private void restartPlayer()
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        player.transform.position = startPosition.transform.position;
    }


    // ******************************************************
    // >>> LÓGICA DE GAME OVER CORREGIDA CON RETRASO <<<
    // ******************************************************
    public void GameOver()
    {
        // 1. Mutear TODO el sonido del juego INMEDIATAMENTE
        AudioListener.volume = 0f;

        // 2. Reproducir el sonido de Game Over INMEDIATAMENTE
        if (gameOverAudioSource != null && gameOverSoundClip != null)
        {
            gameOverAudioSource.PlayOneShot(gameOverSoundClip, gameOverVolume);
            Debug.Log("Sonido de Game Over disparado.");
        }

        // 3. Retrasar la congelación del tiempo y la aparición de la UI.
        // Damos 0.1 segundos (unos 6 frames) para que el motor de audio se inicie antes de congelar el juego.
        Invoke(nameof(ShowGameOverUI), 0.1f);
    }

    private void ShowGameOverUI()
    {
        // 1. Congelar el juego
        Time.timeScale = 0f;

        // 2. Mostrar la pantalla de Game Over
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
    }
    // ******************************************************


    public void RestartGame()
    {
        // Cancelamos la invocación si el jugador hace clic rápido
        CancelInvoke(nameof(ShowGameOverUI));

        // Restaurar volumen y tiempo
        Time.timeScale = 1f;
        AudioListener.volume = 1f;

        LoadCurrentScene();
    }

    public void LoadMainMenu()
    {
        // Restaurar volumen y tiempo
        Time.timeScale = 1f;
        AudioListener.volume = 1f;

        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}