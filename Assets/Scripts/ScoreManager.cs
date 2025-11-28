using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    // --- 1. SINGLETON INSTANCE ---
    // Propiedad estática para acceder al único gestor desde cualquier parte
    public static ScoreManager Instance { get; private set; }

    // --- 2. PROPIEDADES PÚBLICAS ---
    public int startScore = 5000;
    public int scoreDecreaseAmount = 100;
    public float decreaseInterval = 1.5f; // cada cuántos segundos baja el score

    // --- 3. VARIABLES PRIVADAS ---
    public int currentScore;
    private float timer = 0f;
    public TMP_Text scoreText;

    // 4. Implementación del Singleton
    private void Awake()
    {
        // Si ya hay una instancia y no somos nosotros, nos destruimos
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            // Establecemos esta instancia como la única activa
            Instance = this;
            // Opcional: Esto permite que el gestor persista entre escenas
            // DontDestroyOnLoad(this.gameObject); 
        }
    }

    void Start()
    {
        currentScore = startScore;
        UpdateScoreUI();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= decreaseInterval)
        {
            currentScore -= scoreDecreaseAmount;
            if (currentScore < 0) currentScore = 0;

            UpdateScoreUI();
            timer = 0f;
        }
    }

    // Llamar esta función para sumar puntos
    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + currentScore;
    }
}

    