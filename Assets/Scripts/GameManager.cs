using UnityEngine;
using UnityEngine.SceneManagement;



public class GameManager : MonoBehaviour
{
    private string CurrentSceneName;
    public static GameManager Instance;
    
    public GameObject player;
    
    Transform initialPosition;
    public int lifes = 3;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initialPosition = player.transform;
        CurrentSceneName = SceneManager.GetActiveScene().name;
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
        lifes = 3;
    }

    public void HandlePlayerHit()
    {
        

        lifes--;
        

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
            rb.linearVelocity = Vector2.zero;
        }
        player.transform.position = initialPosition.position;
    }
    

}
