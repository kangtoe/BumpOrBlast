using UnityEngine;

public class kangtoe99_Character : MonoBehaviour
{
    // 모든 stat·물리 필드는 SO 주입 — Enemy: kangtoe99_EnemyData, Player: kangtoe99_PlayerStats.
    // 인스펙터에 노출 안 함. 디폴트 값은 SO 미주입 시의 fail-safe가 아니라 단순 초기값.
    protected float moveForce = 50f;
    protected float speedCapOvershoot = 1.5f;
    protected float maxRotationSpeed = 180f;
    protected float maxHealth = 100f;
    protected float collisionKnockbackForce = 3f;

    protected Rigidbody2D rb;
    protected Vector2 moveDirection;
    protected float currentHealth;

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
        LoadStats();
        currentHealth = maxHealth;

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    // 서브클래스가 자기 SO 소스(EnemyData / PlayerStats 등)를 읽어 Character 필드를 채운다.
    // base.Awake() 안에서 rb 확보 직후 / currentHealth 동기화 직전에 호출되므로:
    //   - rb를 만져도 안전 (mass/drag 등)
    //   - maxHealth만 세팅하면 currentHealth는 부모가 알아서 동기화
    // 두 가지 갱신 모델이 공존:
    //   - 스냅샷형 (Enemy): 여기서 필드에 복사, 이후 ApplyTier 등으로 직접 곱
    //   - 라이브-리드형 (Player): 여기서는 maxHealth만 스냅샷, 나머지는 GetEffective* 오버라이드로 매번 SO에서 읽음
    protected virtual void LoadStats() { }

    protected virtual void FixedUpdate()
    {
        Move();
        ClampSpeed();
    }

    /// <summary>
    /// 목표 각도를 향해 제한된 속도로 회전합니다.
    /// </summary>
    protected void RotateTowards(float targetAngle)
    {
        float currentAngle = transform.eulerAngles.z;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, GetEffectiveMaxRotationSpeed() * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
    }

    /// <summary>
    /// 즉시 해당 각도로 회전합니다.
    /// </summary>
    protected void SetRotationImmediate(float angle)
    {
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    // 평형 속도 v_eq = F / (m × d) (Unity 2D 물리). ClampSpeed가 v_eq × speedCapOvershoot로 캡을 건다.
    // 충돌·임펄스 등 외부 힘으로 캡까지는 잠깐 초과 가능, 그 위로는 잘림.
    protected virtual void Move()
    {
        rb.AddForce(moveDirection * GetEffectiveMoveForce());
    }

    // 평형 속도(moveForce, mass, linearDamping 기반) × overshoot로 클램프.
    // drag/mass가 0이면 평형 속도가 정의 안 되므로 클램프 생략.
    protected void ClampSpeed()
    {
        if (rb == null) return;
        float drag = rb.linearDamping;
        float mass = rb.mass;
        if (drag <= 0f || mass <= 0f) return;

        float terminalSpeed = GetEffectiveMoveForce() / (mass * drag);
        float cap = terminalSpeed * GetEffectiveSpeedCapOvershoot();
        if (cap <= 0f) return;

        Vector2 v = rb.linearVelocity;
        if (v.sqrMagnitude > cap * cap)
        {
            rb.linearVelocity = v.normalized * cap;
        }
    }

    protected virtual float GetEffectiveMoveForce() => moveForce;
    protected virtual float GetEffectiveMaxRotationSpeed() => maxRotationSpeed;
    protected virtual float GetEffectiveSpeedCapOvershoot() => speedCapOvershoot;
    protected virtual float GetEffectiveCollisionKnockback() => collisionKnockbackForce;

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

            // 서로에게 넉백 적용 (각자 자기 값을 상대에게 가함 — 비대칭 디자인)
            float myKnockback = GetEffectiveCollisionKnockback();
            ApplyKnockback(knockbackDirection, myKnockback);
            otherCharacter.ApplyKnockback(-knockbackDirection, myKnockback);
        }
    }
}
