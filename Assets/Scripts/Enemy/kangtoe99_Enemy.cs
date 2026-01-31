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
        ChasePlayer();
        CheckDespawnDistance();
    }

    private void ChasePlayer()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        moveDirection = direction;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
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
        // 점수 추가
        if (kangtoe99_ScoreSystem.Instance != null)
        {
            kangtoe99_ScoreSystem.Instance.AddScore(scoreValue);
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
