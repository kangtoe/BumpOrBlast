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

    [Header("Enemy Tier")]
    [SerializeField] private kangtoe99_EnemyTierData tierData;

    [Header("Champion (등급과 별개 축 — 주기적 확률 스폰, 드롭은 챔피언만)")]
    [Tooltip("챔피언 출현 시도 주기(초)")]
    [SerializeField] private float championCheckInterval = 30f;
    [Tooltip("주기마다 챔피언이 실제로 나올 확률")]
    [SerializeField, Range(0f, 1f)] private float championChance = 0.5f;
    [Tooltip("챔피언 수치 배율 (등급 배율 위에 추가)")]
    [SerializeField] private float championStatMultiplier = 3f;
    [Tooltip("챔피언 스케일 배율 (등급 배율 위에 추가)")]
    [SerializeField] private float championScaleMultiplier = 1.8f;

    private Camera mainCamera;
    private float currentSpawnInterval;
    private float spawnTimer;
    private bool isSpawning = false;
    private float currentHealthMultiplier;
    private float elapsedTime;
    private float championTimer;

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

        elapsedTime += Time.deltaTime;

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

        // 챔피언: 일정 주기마다 확률로 추가 스폰 (등급 추첨과 별개 축)
        championTimer -= Time.deltaTime;
        if (championTimer <= 0f)
        {
            championTimer = championCheckInterval;
            if (Random.value < championChance)
            {
                SpawnEnemy(asChampion: true);
            }
        }
    }

    private void SpawnEnemy(bool asChampion = false)
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("Enemy prefabs not assigned!");
            return;
        }

        Vector2 spawnPosition = GetRandomSpawnPosition();
        GameObject randomPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        GameObject enemyObj = Instantiate(randomPrefab, spawnPosition, Quaternion.identity);

        kangtoe99_Enemy enemy = enemyObj.GetComponent<kangtoe99_Enemy>();

        // 등급 배율 (배율 레이어) — 시간 기반 HP 배율보다 먼저 적용
        if (enemy != null && tierData != null)
        {
            enemy.ApplyTier(tierData.PickTier(elapsedTime));
        }

        // 챔피언 강화 (등급과 별개 축)
        if (enemy != null && asChampion)
        {
            enemy.ApplyChampion(championStatMultiplier, championScaleMultiplier);
        }

        // 시간 기반 HP 배율 (등급·챔피언 배율 위에 추가로 곱함)
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
        elapsedTime = 0f;
        championTimer = championCheckInterval;
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
