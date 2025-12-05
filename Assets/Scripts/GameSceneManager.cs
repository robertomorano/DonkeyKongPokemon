using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Cargar la siguiente escena en la lista de Build Settings
    public void LoadNextScene()
    {
        int nextScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1;

        if (nextScene < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
        }
        else
        {
            Debug.Log("Ya no hay más escenas.");
        }
    }

}