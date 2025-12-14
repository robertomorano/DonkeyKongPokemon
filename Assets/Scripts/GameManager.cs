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
            // IMPORTANTE: Para que el GameManager persista entre escenas si fuera necesario
            // DontDestroyOnLoad(gameObject); 
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
        gameOverAudioSource.loop = false;
        gameOverAudioSource.playOnAwake = false;

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
    }

    public void LoadCurrentScene()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }

        lifes = maxLifes;
        isDead = false;

        // Restaurar volumen antes de cargar
        AudioListener.volume = 1f;

        SceneManager.LoadScene(CurrentSceneName);
    }

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
            // Cuando la vida es 0, se llama al método GameOver
            GameOver();
        }
    }

    private void restartPlayer()
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Usar 'velocity' en lugar de 'linearVelocity' es más estándar.
            rb.linearVelocity = Vector2.zero;
        }

        player.transform.position = startPosition.transform.position;
    }

    public void GameOver()
    {
        // 1. Congelar el juego
        Time.timeScale = 0f;

        // 2. Mutear TODOS los sonidos del juego (música, efectos, etc.)
        AudioListener.volume = 0f;

        // 3. Reproducir el sonido de Game Over (debe ignorar el AudioListener.volume = 0f)
        if (gameOverAudioSource != null && gameOverSoundClip != null)
        {
            // Nota importante: Si usas el Audio Mixer, esta fuente debe estar en un grupo no muteado.
            // Si NO usas Audio Mixer, el AudioSource DEBE tener la opción "Ignore Listener Volume" marcada en el Inspector.
            gameOverAudioSource.PlayOneShot(gameOverSoundClip, gameOverVolume);
        }

        // 4. Mostrar la pantalla de Game Over
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
    }

    public void RestartGame()
    {
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