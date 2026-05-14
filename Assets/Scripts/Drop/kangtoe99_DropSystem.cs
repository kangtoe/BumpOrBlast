using UnityEngine;

public class kangtoe99_DropSystem : MonoBehaviour
{
    public static kangtoe99_DropSystem Instance { get; private set; }

    [Header("Drop Prefabs")]
    [SerializeField] private kangtoe99_DropXPOrb xpOrbPrefab;
    [SerializeField] private kangtoe99_DropHealthPack healthPackPrefab;
    [SerializeField] private kangtoe99_DropBomb bombPrefab;

    [Header("Bonus Drop Rates (0-1)")]
    [SerializeField] private float healthPackDropRate = 0.1f; // 10%
    [SerializeField] private float bombDropRate = 0.02f;      // 2%

    [Header("Spawn Conditions")]
    [SerializeField] private int minEnemiesForBomb = 20;           // 폭탄 스폰에 필요한 최소 적 수
    [SerializeField] private float bombCooldown = 180f;            // 폭탄 스폰 쿨다운 (3분)
    [SerializeField] private float healthPackHealthThreshold = 0.5f; // 회복 스폰 체력 조건 (50%)
    [SerializeField] private float healthPackCooldown = 120f;      // 회복 스폰 쿨다운 (2분)

    private float lastBombSpawnTime = float.NegativeInfinity;
    private float lastHealthPackSpawnTime = float.NegativeInfinity;
    private kangtoe99_Character playerCharacter;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerCharacter = playerObj.GetComponent<kangtoe99_Character>();
        }
    }

    public void TryDrop(Vector3 position, Vector2 direction, int scoreValue)
    {
        float roll = Random.value;
        if (roll < bombDropRate && CanSpawnBomb())
        {
            SpawnBomb(position, direction);
            lastBombSpawnTime = Time.time;
        }
        else if (roll < bombDropRate + healthPackDropRate && CanSpawnHealthPack())
        {
            SpawnHealthPack(position, direction);
            lastHealthPackSpawnTime = Time.time;
        }
        else
        {
            SpawnXPOrb(position, direction, scoreValue);
        }
    }

    private bool CanSpawnBomb()
    {
        if (Time.time - lastBombSpawnTime < bombCooldown) return false;
        int enemyCount = FindObjectsByType<kangtoe99_Enemy>(FindObjectsSortMode.None).Length;
        return enemyCount >= minEnemiesForBomb;
    }

    private bool CanSpawnHealthPack()
    {
        if (Time.time - lastHealthPackSpawnTime < healthPackCooldown) return false;
        if (playerCharacter == null) return false;
        return playerCharacter.GetHealthPercentage() <= healthPackHealthThreshold;
    }

    private void SpawnXPOrb(Vector3 position, Vector2 direction, int scoreValue)
    {
        if (xpOrbPrefab == null) return;

        kangtoe99_DropXPOrb xpOrb = Instantiate(xpOrbPrefab, position, Quaternion.identity);
        xpOrb.SetDropDirection(direction);
        xpOrb.SetScoreValue(scoreValue);
    }

    private void SpawnHealthPack(Vector3 position, Vector2 direction)
    {
        if (healthPackPrefab == null) return;

        kangtoe99_DropHealthPack healthPack = Instantiate(healthPackPrefab, position, Quaternion.identity);
        healthPack.SetDropDirection(direction);
    }

    private void SpawnBomb(Vector3 position, Vector2 direction)
    {
        if (bombPrefab == null) return;

        kangtoe99_DropBomb bomb = Instantiate(bombPrefab, position, Quaternion.identity);
        bomb.SetDropDirection(direction);
    }
}
