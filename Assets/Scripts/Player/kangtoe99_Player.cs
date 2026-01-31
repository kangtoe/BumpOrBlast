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

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    public override void TakeDamage(float damage, Vector2? hitPosition = null)
    {
        // 기본 피격 처리 (데미지, 파티클 등)
        base.TakeDamage(damage, hitPosition);
    }

    protected override void Die()
    {
        Debug.Log("Player Died!");

        // 게임 오버 트리거
        if (kangtoe99_GameManager.Instance != null)
        {
            kangtoe99_GameManager.Instance.TriggerGameOver();
        }

        base.Die();
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
}
