using UnityEngine;

public class kangtoe99_Character : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] protected float moveForce = 50f;
    [SerializeField] protected float maxSpeed = 5f;
    protected Rigidbody2D rb;
    protected Vector2 moveDirection;

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

        // 피격 파티클 재생
        if (hitParticlePrefab != null)
        {
            Vector3 spawnPosition = hitPosition.HasValue ? hitPosition.Value : transform.position;
            GameObject hitVFX = Instantiate(hitParticlePrefab, spawnPosition, Quaternion.identity);
            Destroy(hitVFX, 2f); // 2초 후 파티클 오브젝트 삭제
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
