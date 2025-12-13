using UnityEngine;

public class vigorothScript : MonoBehaviour
{
    private Animator animator;

    
    
    public GameObject voltorb;
    public GameObject electrode;

    
    public float minTime = 1f;
    public float maxTime = 3f;

    
    
    public float chanceOfLargeBarrel = 0.2f;



    void Start()
    {
        animator = GetComponent<Animator>();

        Spawn();
    }

    /// <summary>
    /// Función ejecutada por Invoke. Dispara el Trigger, instancia el barril alternado y se programa de nuevo.
    /// </summary>
    private void Spawn()
    {
        
        animator.SetTrigger("Launch");

        
        GameObject prefabToSpawn;

        if (Random.value < chanceOfLargeBarrel)
        {
            prefabToSpawn = electrode;
            
        }
        else
        {
            prefabToSpawn = voltorb;
            
        }

        
        if (prefabToSpawn != null)
        {
            Instantiate(prefabToSpawn, transform.position, Quaternion.identity);
        }
        


        
        


        
        float nextSpawnTime = Random.Range(minTime, maxTime);
        Invoke(nameof(Spawn), nextSpawnTime);
    }
}

