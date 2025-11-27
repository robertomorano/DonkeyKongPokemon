using UnityEngine;

public class Spawner : MonoBehaviour
{

    public GameObject barrel;
    public float minTime = 2.0f;
    public float maxTime = 4.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Spawn();
    }


    private void Spawn()
    {
        Instantiate(barrel, transform.position, Quaternion.identity);
        Invoke(nameof(Spawn), Random.Range(minTime, maxTime));
    }
    
}
