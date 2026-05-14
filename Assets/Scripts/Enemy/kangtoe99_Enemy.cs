using UnityEngine;

public class kangtoe99_Enemy : kangtoe99_Character
{
    [Header("Enemy Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private int scoreValue = 10;
    [SerializeField] private float despawnDistance = 30f;

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
        // 적 격파 시 즉시 점수 획득
        if (kangtoe99_ScoreSystem.Instance != null)
        {
            kangtoe99_ScoreSystem.Instance.AddScore(scoreValue);
        }

        if (kangtoe99_RunStats.Instance != null)
        {
            kangtoe99_RunStats.Instance.AddKill();
        }

        // 드롭
        if (kangtoe99_DropSystem.Instance != null)
        {
            kangtoe99_DropSystem.Instance.TryDrop(transform.position, transform.up, scoreValue);
        }

        Debug.Log($"Enemy died! Score: {scoreValue}");
        base.Die();
    }

    public void Initialize(kangtoe99_EnemyData data)
    {
        if (data == null) return;

        maxSpeed = data.moveSpeed;
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

    public float GetDamage() => damage;
    public int GetScoreValue() => scoreValue;
}
