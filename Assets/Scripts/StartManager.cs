using UnityEngine;
using UnityEngine.SceneManagement;

public class StartManager : MonoBehaviour
{

    public void OnStartClick()
    {
        SceneManager.LoadScene("Fase1");
    }

    public void OnExitClick()
    {
        Application.Quit();
    }
}