using UnityEngine;

public class kangtoe99_Enemy : kangtoe99_Character
{
    [Header("Enemy Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private int scoreValue = 10;
    [SerializeField] private float despawnDistance = 30f;

    // 등급·챔피언 — 스폰 시 EnemySpawner가 적용 (둘은 별개 축).
    private kangtoe99_Tier currentTier = kangtoe99_Tier.Gray;
    private bool isChampion = false;
    public kangtoe99_Tier Tier => currentTier;
    public bool IsChampion => isChampion;

    private Transform player;

    protected override void Awake()
    {
        base.Awake();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogWarning("Enemy: Player not found! Make sure Player has 'Player' tag.");
        }
    }

    private void OnEnable()
    {
        kangtoe99_EnemyRegistry.Register(this);
    }

    private void OnDisable()
    {
        kangtoe99_EnemyRegistry.Unregister(this);
    }

    private void Update()
    {
        CheckDespawnDistance();
        RotateTowardsPlayer();
    }

    private void CheckDespawnDistance()
    {
        if (player == null) return;
        if (Vector2.Distance(transform.position, player.position) > despawnDistance)
        {
            Destroy(gameObject);
        }
    }

    protected override void FixedUpdate()
    {
        // Character의 Move()를 호출하지 않고 직접 AddForce 사용
        ChasePlayer();
        ClampSpeed();
    }

    // 평형 속도는 Rigidbody2D.linearDamping(drag)로만 결정. 프리팹의 drag와 moveForce 비율로 튜닝.
    private void ChasePlayer()
    {
        if (player == null || rb == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.AddForce(direction * moveForce);
    }

    private void RotateTowardsPlayer()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        RotateTowards(targetAngle);
    }

    // 받은 데미지를 RunStats의 "가한 총 피해량"으로 집계 (오버킬 제외).
    public override void TakeDamage(float damage, Vector2? hitPosition = null)
    {
        float effective = Mathf.Min(damage, GetCurrentHealth());
        base.TakeDamage(damage, hitPosition);
        if (effective > 0f && kangtoe99_RunStats.Instance != null)
        {
            kangtoe99_RunStats.Instance.AddDamageDealt(effective);
        }
    }

    protected override void Die()
    {
        if (kangtoe99_RunStats.Instance != null)
        {
            kangtoe99_RunStats.Instance.AddKill();
        }

        // 점수는 더 이상 격파 즉시 가산하지 않는다 — XP orb 픽업 시점에서만 가산.
        // 일반 적은 XP orb만, 챔피언은 XP orb + (폭탄 or 회복) 보너스 1종.
        if (kangtoe99_DropSystem.Instance != null)
        {
            if (isChampion)
            {
                kangtoe99_DropSystem.Instance.DropChampion(transform.position, transform.up, scoreValue);
            }
            else
            {
                kangtoe99_DropSystem.Instance.DropEnemy(transform.position, transform.up, scoreValue);
            }
        }

        Debug.Log($"Enemy died! ScoreValue(orb): {scoreValue}, Champion: {isChampion}");
        base.Die();
    }

    public void Initialize(kangtoe99_EnemyData data)
    {
        if (data == null) return;

        maxHealth = data.maxHealth;
        currentHealth = maxHealth;
        damage = data.damage;
        scoreValue = data.scoreValue;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && data.sprite != null)
        {
            sr.sprite = data.sprite;
            sr.color = data.color;
            originalColor = data.color; // 원래 색상 저장
        }
    }

    // 스폰 시 EnemySpawner가 호출 — 등급 배율을 기존 수치 위에 곱하고 시각을 등급색·크기로 바꾼다.
    // 등급 색상은 공용 팔레트에서 온 값을 EnemySpawner가 넘겨준다.
    public void ApplyTier(kangtoe99_EnemyTierData.TierEntry entry, Color tierColor)
    {
        if (entry == null) return;
        currentTier = entry.tier;

        maxHealth *= entry.statMultiplier;
        currentHealth = maxHealth;
        damage *= entry.statMultiplier;
        moveForce *= entry.statMultiplier;
        scoreValue = Mathf.RoundToInt(scoreValue * entry.scoreMultiplier);

        transform.localScale *= entry.scaleMultiplier;

        SpriteRenderer sr = spriteRenderer != null ? spriteRenderer : GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = tierColor;
            originalColor = tierColor;
        }
    }

    // 스폰 시 EnemySpawner가 호출 — 챔피언은 등급과 별개로 수치·스케일을 추가 강화. 드롭 자격이 생긴다.
    public void ApplyChampion(float statMultiplier, float scaleMultiplier)
    {
        isChampion = true;

        maxHealth *= statMultiplier;
        currentHealth = maxHealth;
        damage *= statMultiplier;
        moveForce *= statMultiplier;
        // mass도 같이 올려 F/m 비율을 유지 — 평형 속도 동일, 가속·넉백 저항만 무거워짐.
        if (rb != null) rb.mass *= statMultiplier;
        scoreValue = Mathf.RoundToInt(scoreValue * statMultiplier);

        transform.localScale *= scaleMultiplier;
    }

    public float GetDamage() => damage;
    public int GetScoreValue() => scoreValue;
}
