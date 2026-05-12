using UnityEngine;

public class kangtoe99_EnemySpawner : MonoBehaviour
{
    public static kangtoe99_EnemySpawner Instance { get; private set; }

    [Header("Spawner Settings")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private Transform player;

    [Header("Spawn Settings")]
    [SerializeField] private float initialSpawnInterval = 3f;
    [SerializeField] private float minSpawnInterval = 0.5f;
    [SerializeField] private float intervalDecreaseRate = 0.05f;
    [SerializeField] private float spawnDistanceFromScreen = 2f;

    [Header("Health Multiplier Settings")]
    [SerializeField] private float initialHealthMultiplier = 1f;
    [SerializeField] private float maxHealthMultiplier = 5f;
    [SerializeField] private float healthMultiplierIncreaseRate = 0.02f;

    private Camera mainCamera;
    private float currentSpawnInterval;
    private float spawnTimer;
    private bool isSpawning = false;
    private float currentHealthMultiplier;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        currentSpawnInterval = initialSpawnInterval;
        spawnTimer = currentSpawnInterval;
        currentHealthMultiplier = initialHealthMultiplier;

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("[kangtoe99_EnemySpawner] Main Camera not found.");
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    private void Update()
    {
        if (!isSpawning || player == null) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            SpawnEnemy();
            spawnTimer = currentSpawnInterval;

            currentSpawnInterval = Mathf.Max(
                currentSpawnInterval - intervalDecreaseRate,
                minSpawnInterval
            );

            currentHealthMultiplier = Mathf.Min(
                currentHealthMultiplier + healthMultiplierIncreaseRate,
                maxHealthMultiplier
            );
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("Enemy prefabs not assigned!");
            return;
        }

        Vector2 spawnPosition = GetRandomSpawnPosition();
        GameObject randomPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        GameObject enemyObj = Instantiate(randomPrefab, spawnPosition, Quaternion.identity);

        kangtoe99_Character character = enemyObj.GetComponent<kangtoe99_Character>();
        if (character != null)
        {
            float newMaxHealth = character.GetMaxHealth() * currentHealthMultiplier;
            character.SetMaxHealth(newMaxHealth);
            character.Heal(newMaxHealth);
        }
    }

    private Vector2 GetRandomSpawnPosition()
    {
        if (mainCamera == null) return Vector2.zero;

        float cameraHeight = mainCamera.orthographicSize;
        float effectiveAspect = kangtoe99_AspectRatioController.GetEffectiveAspectRatio(mainCamera);
        float cameraWidth = cameraHeight * effectiveAspect;
        Vector3 cameraPos = mainCamera.transform.position;

        int side = Random.Range(0, 4);
        Vector2 spawnPosition = Vector2.zero;

        switch (side)
        {
            case 0:
                spawnPosition = new Vector2(
                    Random.Range(cameraPos.x - cameraWidth, cameraPos.x + cameraWidth),
                    cameraPos.y + cameraHeight + spawnDistanceFromScreen
                );
                break;
            case 1:
                spawnPosition = new Vector2(
                    Random.Range(cameraPos.x - cameraWidth, cameraPos.x + cameraWidth),
                    cameraPos.y - cameraHeight - spawnDistanceFromScreen
                );
                break;
            case 2:
                spawnPosition = new Vector2(
                    cameraPos.x - cameraWidth - spawnDistanceFromScreen,
                    Random.Range(cameraPos.y - cameraHeight, cameraPos.y + cameraHeight)
                );
                break;
            case 3:
                spawnPosition = new Vector2(
                    cameraPos.x + cameraWidth + spawnDistanceFromScreen,
                    Random.Range(cameraPos.y - cameraHeight, cameraPos.y + cameraHeight)
                );
                break;
        }

        return spawnPosition;
    }

    public void StartSpawning()
    {
        isSpawning = true;
        currentSpawnInterval = initialSpawnInterval;
        currentHealthMultiplier = initialHealthMultiplier;
        spawnTimer = 0;
        Debug.Log("Enemy spawning started!");
    }

    public void StopSpawning()
    {
        isSpawning = false;
        Debug.Log("Enemy spawning stopped!");
    }

    private void OnDrawGizmosSelected()
    {
        Camera cam = mainCamera != null ? mainCamera : Camera.main;
        if (cam == null) return;

        float cameraHeight = cam.orthographicSize;
        float effectiveAspect = kangtoe99_AspectRatioController.GetEffectiveAspectRatio(cam);
        float cameraWidth = cameraHeight * effectiveAspect;
        Vector3 cameraPos = cam.transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new Vector3(cameraPos.x, cameraPos.y, 0), new Vector3(cameraWidth * 2, cameraHeight * 2, 0));

        Gizmos.color = Color.red;
        float outerWidth = cameraWidth + spawnDistanceFromScreen;
        float outerHeight = cameraHeight + spawnDistanceFromScreen;
        Gizmos.DrawWireCube(new Vector3(cameraPos.x, cameraPos.y, 0), new Vector3(outerWidth * 2, outerHeight * 2, 0));
    }
}
