using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class Spawner : MonoBehaviour
{

    public Animator animator;

    // Asumiendo que estas variables están definidas y asignadas en el Inspector
    public GameObject barrel;
    public float minTime = 1f;
    public float maxTime = 3f;

    void Start()
    {
        animator = GetComponent<Animator>();
        // Llama a la primera ejecución.
        Spawn();
    }

    private void Spawn()
    {
        // 1. Instanciar el objeto
        animator.SetTrigger("Launch");
        

        // 2. Instanciar el objeto (el barril)
        Instantiate(barrel, transform.position, Quaternion.identity);

        // 3. Programar el siguiente Spawn de manera recurrente
        float nextSpawnTime = Random.Range(minTime, maxTime);
        Invoke(nameof(Spawn), nextSpawnTime);
        
    }
    void FixedUpdate()
    {
        
    }
}
