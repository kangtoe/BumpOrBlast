using UnityEngine;

public class kangtoe99_EnemySpawner : MonoBehaviour
{
    public static kangtoe99_EnemySpawner Instance { get; private set; }

    [Header("Spawner Settings")]
    [SerializeField] private GameObject[] enemyPrefabs; // 3종류의 적 프리팹
    [SerializeField] private Transform player;

    [Header("Spawn Settings")]
    [SerializeField] private float initialSpawnInterval = 3f;
    [SerializeField] private float minSpawnInterval = 0.5f;
    [SerializeField] private float intervalDecreaseRate = 0.05f;
    [SerializeField] private float spawnDistance = 15f;

    private float currentSpawnInterval;
    private float spawnTimer;
    private bool isSpawning = false;

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

            // 난이도 증가: 스폰 간격 감소
            currentSpawnInterval = Mathf.Max(
                currentSpawnInterval - intervalDecreaseRate,
                minSpawnInterval
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

        // 랜덤 위치 생성 (플레이어로부터 일정 거리)
        Vector2 spawnPosition = GetRandomSpawnPosition();

        // 랜덤 적 프리팹 선택
        GameObject randomPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        // 적 생성
        Instantiate(randomPrefab, spawnPosition, Quaternion.identity);
    }

    private Vector2 GetRandomSpawnPosition()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnDistance;

        return (Vector2)player.position + offset;
    }

    public void StartSpawning()
    {
        isSpawning = true;
        currentSpawnInterval = initialSpawnInterval;
        spawnTimer = currentSpawnInterval;
        Debug.Log("Enemy spawning started!");
    }

    public void StopSpawning()
    {
        isSpawning = false;
        Debug.Log("Enemy spawning stopped!");
    }

    // 디버그용
    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, spawnDistance);
    }
}
