using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private string CurrentSceneName;
    public static GameManager Instance;

    public GameObject player;
    public GameObject startPosition;
    public GameObject gameOverUI;

    private bool isDead = false;

    public int lifes = 3;

    public Sprite fullLifeSprite;
    public Sprite halfLifeSprite;
    public Sprite lowLifeSprite;

    public SpriteRenderer hudSpriteRenderer;

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
    }

    public void LoadCurrentScene()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }

        lifes = maxLifes;
        isDead = false;

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
            rb.linearVelocity = Vector2.zero;
        }

        player.transform.position = startPosition.transform.position;
    }

    public void GameOver()
    {
        Time.timeScale = 0f;

        if (gameOverUI != null)
        {
            // Esta línea activa tu Canvas de Game Over
            gameOverUI.SetActive(true);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        LoadCurrentScene();
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}