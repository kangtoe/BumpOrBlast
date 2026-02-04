using UnityEngine;

public class kangtoe99_Character : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] protected float moveForce = 50f;
    [SerializeField] protected float maxSpeed = 5f;
    protected Rigidbody2D rb;
    protected Vector2 moveDirection;

    [Header("Rotation")]
    [SerializeField] protected float maxRotationSpeed = 180f; // 초당 최대 회전 각도

    [Header("Health")]
    [SerializeField] protected float maxHealth = 100f;
    protected float currentHealth;

    [Header("Collision")]
    [SerializeField] protected float collisionKnockbackForce = 3f;

    [Header("Visual")]
    [SerializeField] protected SpriteRenderer spriteRenderer;
    protected Color originalColor;

    [Header("VFX")]
    [SerializeField] protected GameObject hitParticlePrefab;
    [SerializeField] protected GameObject deathParticlePrefab;

    [Header("SFX")]
    [SerializeField] protected AudioClip hitSound;
    [SerializeField] protected AudioClip deathSound;    

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    protected virtual void FixedUpdate()
    {
        Move();
    }

    /// <summary>
    /// 목표 각도를 향해 제한된 속도로 회전합니다.
    /// </summary>
    protected void RotateTowards(float targetAngle)
    {
        float currentAngle = transform.eulerAngles.z;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, maxRotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
    }

    /// <summary>
    /// 즉시 해당 각도로 회전합니다.
    /// </summary>
    protected void SetRotationImmediate(float angle)
    {
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    protected virtual void Move()
    {
        // 힘을 가해서 이동
        rb.AddForce(moveDirection * moveForce);

        // 최대 속도 제한
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    public virtual void TakeDamage(float damage, Vector2? hitPosition = null)
    {
        currentHealth -= damage;
        UpdateHealthColor();

        // 피격 사운드 재생
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, Camera.main.transform.position);
        }

        // 피격 파티클 재생
        if (hitParticlePrefab != null)
        {
            Vector3 spawnPosition = hitPosition.HasValue ? hitPosition.Value : transform.position;
            Instantiate(hitParticlePrefab, spawnPosition, Quaternion.identity);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void ApplyKnockback(Vector2 direction, float force)
    {
        if (rb != null)
        {
            rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
        }
    }

    public virtual void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHealthColor();
    }

    protected void UpdateHealthColor()
    {
        if (spriteRenderer == null) return;

        float healthRatio = GetHealthPercentage();
        // 체력 비율에 따라 원래 색상에서 검정색으로 보간
        Color targetColor = Color.Lerp(Color.black, originalColor, healthRatio);
        spriteRenderer.color = targetColor;
    }

    protected virtual void Die()
    {
        // 사망 사운드 재생
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, Camera.main.transform.position);
        }

        // 사망 파티클 재생
        if (deathParticlePrefab != null)
        {
            GameObject deathVFX = Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
            Destroy(deathVFX, 3f); // 3초 후 파티클 오브젝트 삭제
        }

        Destroy(gameObject);
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercentage() => currentHealth / maxHealth;

    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }

    public float GetMoveSpeed() => maxSpeed;

    public void SetMoveSpeed(float newSpeed)
    {
        maxSpeed = newSpeed;
    }

    public void SetMoveForce(float newForce)
    {
        moveForce = newForce;
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        kangtoe99_Character otherCharacter = collision.gameObject.GetComponent<kangtoe99_Character>();
        if (otherCharacter != null)
        {
            // 충돌 방향 계산 (상대방 -> 나)
            Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;

            // 서로에게 넉백 적용
            ApplyKnockback(knockbackDirection, collisionKnockbackForce);
            otherCharacter.ApplyKnockback(-knockbackDirection, collisionKnockbackForce);
        }
    }
}
