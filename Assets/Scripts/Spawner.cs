using UnityEngine;

public class Spawner : MonoBehaviour
{

    public Animator animator;
    public GameObject barrel;
    public float minTime = 2.0f;
    public float maxTime = 4.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        Spawn();
    }

    private void Spawn()
    {
        // 1. Instanciar el objeto
        Instantiate(barrel, transform.position, Quaternion.identity);

        // 2. Disparar el Trigger (la animación se ejecuta y se resetea automáticamente)
        if (animator != null)
        {
            animator.SetTrigger("Launch"); // Usa el nombre de tu Trigger aquí
        }

        // 3. Re-invocar para la siguiente aparición
        Invoke(nameof(Spawn), Random.Range(minTime, maxTime));
    }
    
}
