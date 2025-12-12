using UnityEngine;
using UnityEngine.SceneManagement;

public class Final : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("¡Nivel completado!");

            // 1. Obtener el índice de la escena actual
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

            // 2. Calcular el índice de la siguiente escena
            int nextSceneIndex = currentSceneIndex + 1;

            // **OPCIONAL:** Puedes añadir una verificación para ver si es la última escena.
            // Si nextSceneIndex es igual o mayor al conteo total de escenas, 
            // puedes cargar un menú principal, una pantalla de victoria, o volver a la primera fase.
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                // 3. Cargar la siguiente escena usando su índice
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.Log("¡Juego completado o última fase!");
                // Ejemplo: Cargar la primera escena (índice 0) o una escena de "Victoria"
                // SceneManager.LoadScene(0); 
                // SceneManager.LoadScene("PantallaVictoria");
            }
        }
    }
}