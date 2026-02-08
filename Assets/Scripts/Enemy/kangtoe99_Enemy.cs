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
        else
        {
            Debug.Log("Enemy: Player found!");
        }
    }

    private void Update()
    {
        CheckDespawnDistance();
        RotateTowardsPlayer();
    }

    protected override void FixedUpdate()
    {
        // Character의 Move()를 호출하지 않고 직접 AddForce 사용
        ChasePlayer();
    }

    private void ChasePlayer()
    {
        if (player == null || rb == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.AddForce(direction * moveForce);

        // 최대 속도 제한
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    private void RotateTowardsPlayer()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        RotateTowards(targetAngle);
    }

    private void CheckDespawnDistance()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > despawnDistance)
        {
            Destroy(gameObject);
        }
    }

    protected override void Die()
    {
        // 적 격파 시 즉시 점수 획득
        if (kangtoe99_ScoreSystem.Instance != null)
        {
            kangtoe99_ScoreSystem.Instance.AddScore(scoreValue);
        }

        // 아이템 드롭
        if (kangtoe99_ItemDropSystem.Instance != null)
        {
            kangtoe99_ItemDropSystem.Instance.TryDropItem(transform.position, transform.up, scoreValue);
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
