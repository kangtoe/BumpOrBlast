using UnityEngine;

public class kangtoe99_ItemDropSystem : MonoBehaviour
{
    public static kangtoe99_ItemDropSystem Instance { get; private set; }

    [Header("Item Prefabs")]
    [SerializeField] private kangtoe99_ItemXPOrb xpOrbPrefab;
    [SerializeField] private kangtoe99_ItemHealthPack healthPackPrefab;
    [SerializeField] private kangtoe99_ItemBomb bombPrefab;

    [Header("Bonus Drop Rates (0-1)")]
    [SerializeField] private float healthPackDropRate = 0.1f; // 10%
    [SerializeField] private float bombDropRate = 0.02f;      // 2%

    [Header("Spawn Conditions")]
    [SerializeField] private int minEnemiesForBomb = 20;           // 폭탄 스폰에 필요한 최소 적 수
    [SerializeField] private float bombCooldown = 180f;            // 폭탄 스폰 쿨다운 (3분)
    [SerializeField] private float healthPackHealthThreshold = 0.5f; // 회복 스폰 체력 조건 (50%)
    [SerializeField] private float healthPackCooldown = 120f;      // 회복 스폰 쿨다운 (2분)

    private float lastBombSpawnTime = float.NegativeInfinity;      // 마지막 폭탄 스폰 시간
    private float lastHealthPackSpawnTime = float.NegativeInfinity; // 마지막 회복 스폰 시간
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
        // 플레이어 캐릭터 찾기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerCharacter = playerObj.GetComponent<kangtoe99_Character>();
        }
    }

    public void TryDropItem(Vector3 position, Vector2 direction, int scoreValue)
    {
        // 아이템 확률 드롭
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
            // XP Orb는 기본 드롭
            SpawnXPOrb(position, direction, scoreValue);
        }
    }

    private bool CanSpawnBomb()
    {
        // 쿨다운 체크 (3분)
        if (Time.time - lastBombSpawnTime < bombCooldown) return false;

        // 적 20기 이상
        int enemyCount = FindObjectsByType<kangtoe99_Enemy>(FindObjectsSortMode.None).Length;
        return enemyCount >= minEnemiesForBomb;
    }

    private bool CanSpawnHealthPack()
    {
        // 쿨다운 체크 (2분)
        if (Time.time - lastHealthPackSpawnTime < healthPackCooldown) return false;

        // 플레이어 체력 50% 이하
        if (playerCharacter == null) return false;
        return playerCharacter.GetHealthPercentage() <= healthPackHealthThreshold;
    }

    private void SpawnXPOrb(Vector3 position, Vector2 direction, int scoreValue)
    {
        if (xpOrbPrefab == null) return;

        kangtoe99_ItemXPOrb xpOrb = Instantiate(xpOrbPrefab, position, Quaternion.identity);
        xpOrb.SetDropDirection(direction);
        xpOrb.SetScoreValue(scoreValue);
    }

    private void SpawnHealthPack(Vector3 position, Vector2 direction)
    {
        if (healthPackPrefab == null) return;

        kangtoe99_ItemHealthPack healthPack = Instantiate(healthPackPrefab, position, Quaternion.identity);
        healthPack.SetDropDirection(direction);
    }

    private void SpawnBomb(Vector3 position, Vector2 direction)
    {
        if (bombPrefab == null) return;

        kangtoe99_ItemBomb bomb = Instantiate(bombPrefab, position, Quaternion.identity);
        bomb.SetDropDirection(direction);
    }
}
