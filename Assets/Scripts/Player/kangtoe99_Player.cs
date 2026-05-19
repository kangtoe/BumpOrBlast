using UnityEngine;

[RequireComponent(typeof(kangtoe99_PlayerStats))]
public class kangtoe99_Player : kangtoe99_Character
{
    [Header("Camera (for screen wrap-around)")]
    [SerializeField] private Camera mainCamera;

    private kangtoe99_IRotationInput rotationInput;
    private kangtoe99_PlayerStats stats;
    private Vector3 originalScale;

    public kangtoe99_PlayerStats Stats => stats;

    protected override void Awake()
    {
        base.Awake();

        originalScale = transform.localScale;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        rotationInput = GetComponent<kangtoe99_IRotationInput>();
        if (rotationInput == null)
        {
            Debug.LogWarning("[kangtoe99_Player] kangtoe99_IRotationInput 구현체가 필요합니다.");
        }

        ApplyBodyScale();
        ApplyFriction();
        stats.OnStatChanged += OnStatChanged;
    }

    protected override void LoadStats()
    {
        stats = GetComponent<kangtoe99_PlayerStats>();
        // maxHealth만 스냅샷 — 나머지(moveForce, rotation 등)는 GetEffective* 오버라이드로 라이브 리드.
        // currentHealth = maxHealth 동기화는 base.Awake가 LoadStats 호출 직후 처리.
        maxHealth = stats.GetFinal(kangtoe99_StatType.MaxHP);
    }

    private void OnDestroy()
    {
        stats.OnStatChanged -= OnStatChanged;
    }

    private void OnStatChanged(kangtoe99_StatType stat)
    {
        switch (stat)
        {
            case kangtoe99_StatType.MaxHP:
                float prevMax = GetMaxHealth();
                float newMax = stats.GetFinal(kangtoe99_StatType.MaxHP);
                SetMaxHealth(newMax);
                if (newMax > prevMax) Heal(newMax - prevMax);
                break;

            case kangtoe99_StatType.BodyScale:
                ApplyBodyScale();
                break;

            case kangtoe99_StatType.Friction:
                ApplyFriction();
                break;
        }
    }

    private void ApplyBodyScale()
    {
        float scale = stats.GetFinal(kangtoe99_StatType.BodyScale);
        transform.localScale = originalScale * scale;
    }

    private void ApplyFriction()
    {
        rb.linearDamping = stats.GetFinal(kangtoe99_StatType.Friction);
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        // 시작 전에도 이동/회전은 허용 — 적이 안 나오는 상태에서 조작감을 미리 익힐 수 있게.
        // HP 리젠은 게임 시작 이후에만 동작 (시작 전엔 의미가 없고, 게임 흐름 일관성 유지).
        bool started = kangtoe99_GameManager.Instance == null || kangtoe99_GameManager.Instance.IsGameStarted();
        if (started) HandleHPRegen();

        HandleInput();
        HandleRotation();
    }

    private void HandleHPRegen()
    {
        float regen = stats.GetFinal(kangtoe99_StatType.HPRegen);
        if (regen <= 0f) return;
        if (GetCurrentHealth() >= GetMaxHealth()) return;
        Heal(regen * Time.deltaTime);
    }

    private void LateUpdate()
    {
        WrapAroundScreen();
    }

    private void HandleInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector2(horizontal, vertical).normalized;
    }

    private void HandleRotation()
    {
        if (rotationInput == null) return;

        Vector2 direction = rotationInput.GetTargetDirection(transform.position);
        if (direction.sqrMagnitude < 0.0001f) return;

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        RotateTowards(targetAngle);
    }

    private void WrapAroundScreen()
    {
        if (mainCamera == null) return;

        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);
        bool teleported = false;

        if (viewportPosition.x > 1f)
        {
            viewportPosition.x = 0f;
            teleported = true;
        }
        else if (viewportPosition.x < 0f)
        {
            viewportPosition.x = 1f;
            teleported = true;
        }

        if (viewportPosition.y > 1f)
        {
            viewportPosition.y = 0f;
            teleported = true;
        }
        else if (viewportPosition.y < 0f)
        {
            viewportPosition.y = 1f;
            teleported = true;
        }

        if (teleported)
        {
            Vector3 newPosition = mainCamera.ViewportToWorldPoint(viewportPosition);
            newPosition.z = transform.position.z;
            transform.position = newPosition;

            if (rotationInput != null)
            {
                Vector2 direction = rotationInput.GetTargetDirection(newPosition);
                if (direction.sqrMagnitude >= 0.0001f)
                {
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                    SetRotationImmediate(angle);
                }
            }
        }
    }

    protected override float GetEffectiveMoveForce()
        => stats.GetFinal(kangtoe99_StatType.MoveForce);

    protected override float GetEffectiveMaxRotationSpeed()
        => stats.GetFinal(kangtoe99_StatType.RotationSpeed);

    protected override float GetEffectiveSpeedCapOvershoot()
        => stats.GetFinal(kangtoe99_StatType.SpeedCapOvershoot);

    protected override float GetEffectiveCollisionKnockback()
        => stats.GetFinal(kangtoe99_StatType.CollisionKnockback);

    // 받은 데미지를 RunStats의 "받은 총 피해량"으로 집계 (오버킬 제외).
    public override void TakeDamage(float damage, Vector2? hitPosition = null)
    {
        float effective = Mathf.Min(damage, GetCurrentHealth());
        base.TakeDamage(damage, hitPosition);
        if (effective > 0f && kangtoe99_RunStats.Instance != null)
        {
            kangtoe99_RunStats.Instance.AddDamageTaken(effective);
        }
    }

    protected override void Die()
    {
        Debug.Log("Player Died!");

        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, Camera.main.transform.position);
        }

        if (kangtoe99_GameManager.Instance != null)
        {
            kangtoe99_GameManager.Instance.TriggerGameOver();
        }

        if (deathParticlePrefab != null)
        {
            GameObject deathVFX = Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
            Destroy(deathVFX, 3f);
        }

        // Destroy 대신 비활성화 — 게임오버 정보 패널이 ItemInventory(빌드 표시)를 계속 참조해야 한다.
        // 비활성 GameObject는 Update/물리가 멈추지만 컴포넌트는 살아 있어 참조가 유효하다.
        // 씬 재시작(GameManager.RestartGame)에서 씬과 함께 정리된다.
        gameObject.SetActive(false);
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);

        if (collision.gameObject.CompareTag("Enemy"))
        {
            kangtoe99_Enemy enemy = collision.gameObject.GetComponent<kangtoe99_Enemy>();
            if (enemy != null)
            {
                Vector2 hitPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : (Vector2)transform.position;
                TakeDamage(enemy.GetDamage(), hitPoint);
            }
        }
    }
}
