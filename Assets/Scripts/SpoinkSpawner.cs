using UnityEngine;

public class SpoinkSpawner : MonoBehaviour
{
    [Header("Barrel Configuration")]
    [Tooltip("The prefab for the Jumping Barrel (BarrelJumping.cs)")]
    public GameObject jumpingBarrelPrefab;

    [Header("Spawn Timing")]
    public float minTime = 3f; // Often slower than rolling barrels
    public float maxTime = 6f;

    void Start()
    {
        // Start the recurring spawn sequence
        Spawn();
    }

    private void Spawn()
    {
        if (jumpingBarrelPrefab == null)
        {
            Debug.LogError("Jumping Barrel Prefab is not assigned on " + gameObject.name);
            return;
        }

        // 1. Instantiate the jumping barrel
        Instantiate(jumpingBarrelPrefab, transform.position, Quaternion.identity);

        // 2. Schedule the next Spawn recursively
        float nextSpawnTime = Random.Range(minTime, maxTime);
        Invoke(nameof(Spawn), nextSpawnTime);
    }
}
