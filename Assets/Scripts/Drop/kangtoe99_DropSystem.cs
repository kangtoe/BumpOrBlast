using UnityEngine;

public class kangtoe99_DropSystem : MonoBehaviour
{
    public static kangtoe99_DropSystem Instance { get; private set; }

    [Header("Drop Prefabs")]
    [SerializeField] private kangtoe99_DropXPOrb xpOrbPrefab;
    [SerializeField] private kangtoe99_DropHealthPack healthPackPrefab;
    [SerializeField] private kangtoe99_DropBomb bombPrefab;

    [Header("Champion Bonus Weights (HP 비율 기반)")]
    [Tooltip("HP가 가득 찼을 때(=1) 폭탄 선택 가중치. HP비율과 곱해진다.")]
    [SerializeField] private float championBombWeightAtFullHP = 1f;
    [Tooltip("HP가 0일 때(=1) 회복 선택 가중치. (1 - HP비율)과 곱해진다.")]
    [SerializeField] private float championHealWeightAtZeroHP = 1f;

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

    // 일반 적 처치 — XP orb 한 개만 떨어뜨린다.
    public void DropEnemy(Vector3 position, Vector2 direction, int scoreValue)
    {
        SpawnXPOrb(position, direction, scoreValue);
    }

    // 챔피언 처치 — XP orb + 보너스 1종(폭탄 or 회복). 보너스 선택은 플레이어 HP 비율 가중치.
    public void DropChampion(Vector3 position, Vector2 direction, int scoreValue)
    {
        SpawnXPOrb(position, direction, scoreValue);
        SpawnChampionBonus(position, direction);
    }

    // HP가 높을수록 폭탄, 낮을수록 회복 — 선형 가중. 양쪽 합이 0이면 폴백 없이 생략.
    private void SpawnChampionBonus(Vector3 position, Vector2 direction)
    {
        float hpRatio = playerCharacter != null ? Mathf.Clamp01(playerCharacter.GetHealthPercentage()) : 1f;
        float bombWeight = championBombWeightAtFullHP * hpRatio;
        float healWeight = championHealWeightAtZeroHP * (1f - hpRatio);
        float total = bombWeight + healWeight;
        if (total <= 0f) return;

        if (Random.value < bombWeight / total)
        {
            SpawnBomb(position, direction);
        }
        else
        {
            SpawnHealthPack(position, direction);
        }
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
