using UnityEngine;

public class kangtoe99_Player : kangtoe99_Character
{
    private kangtoe99_IRotationInput rotationInput;

    protected override void Awake()
    {
        base.Awake();

        rotationInput = GetComponent<kangtoe99_IRotationInput>();
        if (rotationInput == null)
        {
            Debug.LogWarning("[kangtoe99_Player] kangtoe99_IRotationInput 구현체가 필요합니다. PlayerCharacter에 kangtoe99_MouseRotationInput 컴포넌트를 추가하세요.");
        }

        if (rb != null && rb.interpolation != RigidbodyInterpolation2D.Interpolate)
        {
            Debug.LogWarning($"[kangtoe99_Player] Rigidbody2D.interpolation이 '{rb.interpolation}'입니다. 카메라 추적 떨림 방지를 위해 'Interpolate'로 설정하세요.");
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        if (kangtoe99_GameManager.Instance != null && !kangtoe99_GameManager.Instance.IsGameStarted()) return;

        HandleInput();
        HandleRotation();
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

        Destroy(gameObject);
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
