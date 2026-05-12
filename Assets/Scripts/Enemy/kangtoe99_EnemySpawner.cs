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

    [Header("Open Field (Player-Centered)")]
    [Tooltip("스폰 거리 — 플레이어 중심 원주. 카메라 뷰 바깥이어야 자연스러움")]
    [SerializeField] private float spawnRadius = 18f;
    [Tooltip("이 거리 초과 시 반대편 원주로 재배치 (무한 오픈 필드 효과)")]
    [SerializeField] private float recycleRadius = 28f;

    [Header("Health Multiplier Settings")]
    [SerializeField] private float initialHealthMultiplier = 1f;
    [SerializeField] private float maxHealthMultiplier = 5f;
    [SerializeField] private float healthMultiplierIncreaseRate = 0.02f;

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

        RecycleFarEnemies();

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

    // 거리 초과 적을 플레이어 기준 반대편 원주로 재배치 (삭제 안 함 → 무한감)
    private void RecycleFarEnemies()
    {
        float recycleRadiusSq = recycleRadius * recycleRadius;
        Vector2 playerPos = player.position;
        var enemies = kangtoe99_EnemyRegistry.ActiveEnemies;

        for (int i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (enemy == null) continue;

            Vector2 enemyPos = enemy.transform.position;
            Vector2 offset = enemyPos - playerPos;
            if (offset.sqrMagnitude <= recycleRadiusSq) continue;

            // 반대 방향으로 스폰 반경 거리에 재배치
            Vector2 oppositeDir = -offset.normalized;
            Vector2 newPos = playerPos + oppositeDir * spawnRadius;

            enemy.transform.position = newPos;

            // 관성 초기화 (이전 추적 속도가 반대편에서 이상하게 작용 안 하도록)
            var rb = enemy.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
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

    // 플레이어 중심 원주상의 랜덤 위치
    private Vector2 GetRandomSpawnPosition()
    {
        if (player == null) return Vector2.zero;

        float angle = Random.Range(0f, Mathf.PI * 2f);
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnRadius;
        return (Vector2)player.position + offset;
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
        if (player == null) return;

        // 스폰 원주 (녹색)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(player.position, spawnRadius);

        // 리사이클 경계 (빨간색)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, recycleRadius);
    }
}
