using UnityEngine;

public class kangtoe99_Player : kangtoe99_Character
{
    [Header("Player Settings")]
    [SerializeField] private Camera mainCamera;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        // 게임 일시 정지 중에는 입력 무시
        if (Time.timeScale == 0f) return;

        HandleInput();
        RotateTowardsMouse();
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

    private void RotateTowardsMouse()
    {
        if (mainCamera == null) return;

        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - transform.position).normalized;

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        RotateTowards(targetAngle);
    }

    public override void TakeDamage(float damage, Vector2? hitPosition = null)
    {
        // 기본 피격 처리 (데미지, 파티클 등)
        base.TakeDamage(damage, hitPosition);
    }

    protected override void Die()
    {
        Debug.Log("Player Died!");

        // 사망 사운드를 타임스케일 변경 전에 먼저 재생 (끊김 방지)
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, Camera.main.transform.position);
        }

        // 게임 오버 트리거 (여기서 타임스케일이 변경됨)
        if (kangtoe99_GameManager.Instance != null)
        {
            kangtoe99_GameManager.Instance.TriggerGameOver();
        }

        // 사망 파티클 재생 (슬로우 모션 적용됨)
        if (deathParticlePrefab != null)
        {
            GameObject deathVFX = Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
            Destroy(deathVFX, 3f);
        }

        Destroy(gameObject);
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision); // 넉백 처리

        if (collision.gameObject.CompareTag("Enemy"))
        {
            kangtoe99_Enemy enemy = collision.gameObject.GetComponent<kangtoe99_Enemy>();
            if (enemy != null)
            {
                // 충돌 지점 계산 (첫 번째 접촉점 사용)
                Vector2 hitPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : (Vector2)transform.position;
                TakeDamage(enemy.GetDamage(), hitPoint);
            }
        }
    }

    private void WrapAroundScreen()
    {
        if (mainCamera == null) return;

        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);
        bool teleported = false;

        // 화면을 완전히 벗어났는지 확인하고 반대편으로 이동
        if (viewportPosition.x > 1f)
        {
            // 오른쪽 경계를 벗어남 -> 왼쪽에서 등장
            viewportPosition.x = 0f;
            teleported = true;
        }
        else if (viewportPosition.x < 0f)
        {
            // 왼쪽 경계를 벗어남 -> 오른쪽에서 등장
            viewportPosition.x = 1f;
            teleported = true;
        }

        if (viewportPosition.y > 1f)
        {
            // 위쪽 경계를 벗어남 -> 아래쪽에서 등장
            viewportPosition.y = 0f;
            teleported = true;
        }
        else if (viewportPosition.y < 0f)
        {
            // 아래쪽 경계를 벗어남 -> 위쪽에서 등장
            viewportPosition.y = 1f;
            teleported = true;
        }

        if (teleported)
        {
            Vector3 newPosition = mainCamera.ViewportToWorldPoint(viewportPosition);
            newPosition.z = transform.position.z; // Z 좌표는 유지
            transform.position = newPosition;

            // 텔레포트 후 마우스 방향으로 즉시 회전
            Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mousePosition - newPosition).normalized;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            SetRotationImmediate(targetAngle);
        }
    }
}
