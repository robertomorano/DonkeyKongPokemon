using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class Spawner : MonoBehaviour
{

    [Header("Components & Animation")]
    public Animator animator;

    private AudioSource audioSource;

    [Header("Barrel Prefabs")]
    [Tooltip("The standard barrel (higher chance of spawning).")]
    public GameObject normalBarrelPrefab;
    [Tooltip("The explosive barrel (lower chance of spawning).")]
    public GameObject explosiveBarrelPrefab;

    [Header("Spawn Configuration")]
    [Range(0f, 1f)]
    [Tooltip("The probability (0 to 1) that the Explosive Barrel will spawn. Example: 0.25 means 25% chance.")]
    public float explosiveSpawnChance = 0.25f;

    [Header("Spawn Timing")]
    public float minTime = 1f;
    public float maxTime = 4f;

    [Header("Audio")]
    [Tooltip("The sound played when a barrel is launched.")]
    public AudioClip launchSound;

    void Start()
    {
        // Ensure the Animator component is found if not assigned in the Inspector
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // Añadir el AudioSource si no existe (opcional, mejor añadirlo manualmente)
            // audioSource = gameObject.AddComponent<AudioSource>();
            Debug.LogWarning("AudioSource component missing on Spawner. Audio will not play.");
        }

        // Start the recurring spawn sequence
        Spawn();
    }

    private GameObject SelectBarrelPrefab()
    {
        if (normalBarrelPrefab == null)
        {
            Debug.LogError("Normal Barrel Prefab is not assigned.");
            return null;
        }

        // Generate a random value between 0.0 and 1.0
        float randomValue = Random.value;

        // Check if the random value falls within the explosive chance range
        if (randomValue < explosiveSpawnChance)
        {
            // The explosive barrel should spawn
            if (explosiveBarrelPrefab != null)
            {
                return explosiveBarrelPrefab;
            }
            else
            {
                Debug.LogWarning("Explosive Barrel Chance hit, but prefab is missing. Spawning Normal Barrel instead.");
                return normalBarrelPrefab;
            }
        }
        else
        {
            // The normal barrel should spawn (100% - explosiveSpawnChance)
            return normalBarrelPrefab;
        }
    }

    private void Spawn()
    {
        GameObject barrelToSpawn = SelectBarrelPrefab();

        float nextSpawnTime;

        if (barrelToSpawn == null)
        {
            // Fallback to schedule the next spawn if prefabs were missing
            nextSpawnTime = Random.Range(minTime, maxTime);
            Invoke(nameof(Spawn), nextSpawnTime);
            return;
        }

        if (audioSource != null && launchSound != null)
        {
            audioSource.PlayOneShot(launchSound);
        }

        // 1. Trigger the launch animation
        if (animator != null)
        {
            animator.SetTrigger("Launch");
        }

        // 2. Instantiate the selected barrel
        Instantiate(barrelToSpawn, transform.position, Quaternion.identity);

        // 3. Schedule the next Spawn recursively
        nextSpawnTime = Random.Range(minTime, maxTime);
        Invoke(nameof(Spawn), nextSpawnTime);
    }

}

