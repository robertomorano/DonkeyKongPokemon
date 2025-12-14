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

    [Header("Audio de Golpe")]
    public AudioClip hitSoundClip;
    [Range(0f, 1f)] public float hitVolume = 0.8f;

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
        // Inicialización del AudioSource para Game Over
        if (gameOverAudioSource == null)
        {
            gameOverAudioSource = GetComponent<AudioSource>();
            if (gameOverAudioSource == null)
            {
                gameOverAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        if (gameOverAudioSource != null)
        {
            gameOverAudioSource.loop = false;
            gameOverAudioSource.playOnAwake = false;
            // Necesario para que el sonido de Game Over ignore el Time.timeScale=0
            gameOverAudioSource.ignoreListenerPause = true;
        }

        if (player != null && startPosition != null)
        {
            player.transform.position = startPosition.transform.position;
        }

        // Guarda el nombre de la escena actual
        CurrentSceneName = SceneManager.GetActiveScene().name;
        maxLifes = lifes;

        UpdateHUDVisual();

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }

        AudioListener.volume = 1f;
        Time.timeScale = 1f;
    }

    public void LoadCurrentScene()
    {
        CancelInvoke(nameof(ShowGameOverUI));

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }

        lifes = maxLifes;
        isDead = false;

        Time.timeScale = 1f;
        AudioListener.volume = 1f;

        // Carga la escena guardada en Start()
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

        if (hitSoundClip != null)
        {
            AudioSource.PlayClipAtPoint(hitSoundClip, player.transform.position, hitVolume);
        }

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


    public void GameOver()
    {
        AudioListener.volume = 0f;

        if (gameOverAudioSource != null && gameOverSoundClip != null)
        {
            gameOverAudioSource.PlayOneShot(gameOverSoundClip, gameOverVolume);
        }

        Invoke(nameof(ShowGameOverUI), 0.1f);
    }

    private void ShowGameOverUI()
    {
        Time.timeScale = 0f;

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
    }


    public void RestartGame()
    {
        CancelInvoke(nameof(ShowGameOverUI));

        Time.timeScale = 1f;
        AudioListener.volume = 1f;

        LoadCurrentScene();
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.volume = 1f;

        // ESTA LÍNEA ES DONDE PUEDE ESTAR EL ERROR DE CONFIGURACIÓN
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}