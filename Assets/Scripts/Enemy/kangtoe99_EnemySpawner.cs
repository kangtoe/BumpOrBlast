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
    [SerializeField] private float spawnDistanceFromScreen = 2f; // 화면 밖 거리

    private Camera mainCamera;

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

        // 메인 카메라 찾기
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found!");
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
        if (mainCamera == null) return player.position;

        // 화면 경계를 월드 좌표로 계산
        float cameraHeight = mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        Vector3 cameraPos = mainCamera.transform.position;

        // 랜덤하게 4개 방향 중 하나 선택 (위, 아래, 왼쪽, 오른쪽)
        int side = Random.Range(0, 4);
        Vector2 spawnPosition = Vector2.zero;

        switch (side)
        {
            case 0: // 위쪽
                spawnPosition = new Vector2(
                    Random.Range(cameraPos.x - cameraWidth, cameraPos.x + cameraWidth),
                    cameraPos.y + cameraHeight + spawnDistanceFromScreen
                );
                break;
            case 1: // 아래쪽
                spawnPosition = new Vector2(
                    Random.Range(cameraPos.x - cameraWidth, cameraPos.x + cameraWidth),
                    cameraPos.y - cameraHeight - spawnDistanceFromScreen
                );
                break;
            case 2: // 왼쪽
                spawnPosition = new Vector2(
                    cameraPos.x - cameraWidth - spawnDistanceFromScreen,
                    Random.Range(cameraPos.y - cameraHeight, cameraPos.y + cameraHeight)
                );
                break;
            case 3: // 오른쪽
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
        spawnTimer = 0; // 첫 스폰은 즉시 실행
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
        Camera cam = mainCamera != null ? mainCamera : Camera.main;
        if (cam == null) return;

        float cameraHeight = cam.orthographicSize;
        float cameraWidth = cameraHeight * cam.aspect;
        Vector3 cameraPos = cam.transform.position;

        // 카메라 뷰포트 (녹색)
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            new Vector3(cameraPos.x, cameraPos.y, 0),
            new Vector3(cameraWidth * 2, cameraHeight * 2, 0)
        );

        // 스폰 영역 (빨간색)
        Gizmos.color = Color.red;
        float outerWidth = cameraWidth + spawnDistanceFromScreen;
        float outerHeight = cameraHeight + spawnDistanceFromScreen;
        Gizmos.DrawWireCube(
            new Vector3(cameraPos.x, cameraPos.y, 0),
            new Vector3(outerWidth * 2, outerHeight * 2, 0)
        );
    }
}
