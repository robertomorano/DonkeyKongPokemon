using UnityEngine;
using UnityEngine.SceneManagement;

public class StartManager : MonoBehaviour
{

    public void OnStartClick()
    {
        SceneManager.LoadScene("testMap");
    }

    public void OnExitClick()
    {
        Application.Quit();
    }
}