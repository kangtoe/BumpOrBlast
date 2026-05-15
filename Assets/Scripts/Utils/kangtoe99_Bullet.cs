using UnityEngine;

public class kangtoe99_Bullet : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;
    [SerializeField, Range(0f, 1f)]
    [Tooltip("이 비율 시점부터 작아지며 페이드 — 0=발사 즉시 시작, 0.5=수명 후반 50% 동안 디케이")]
    private float decayStartRatio = 0.5f;

    private float damage = 10f;
    private float speed;
    private float knockbackForce;
    private int pierceRemaining;

    private Vector2 direction;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector3 baseScale;
    private Color baseColor;
    private float spawnTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Vector2 dir, float bulletSpeed, float knockback, float bulletDamage, int pierce = 0)
    {
        direction = dir.normalized;
        speed = bulletSpeed;
        knockbackForce = knockback;
        damage = bulletDamage;
        pierceRemaining = Mathf.Max(0, pierce);

        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }

        // 디케이 베이스 — 스포너가 bulletScale 곱한 뒤의 최종 scale/color 캡처
        spawnTime = Time.time;
        baseScale = transform.localScale;
        if (sr != null) baseColor = sr.color;

        Destroy(gameObject, lifetime);
    }

    // 수명에 따라 작아지며 페이드 아웃 — decayStartRatio 시점부터 시작해 lifetime에서 0이 됨.
    private void Update()
    {
        if (lifetime <= 0f) return;
        float ageRatio = (Time.time - spawnTime) / lifetime;
        if (ageRatio <= decayStartRatio) return;

        float decayWindow = Mathf.Max(0.0001f, 1f - decayStartRatio);
        float remaining = 1f - Mathf.Clamp01((ageRatio - decayStartRatio) / decayWindow);
        transform.localScale = baseScale * remaining;
        if (sr != null)
        {
            Color c = baseColor;
            c.a = baseColor.a * remaining;
            sr.color = c;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return;

        var enemy = collision.GetComponent<kangtoe99_Character>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, transform.position);
            enemy.ApplyKnockback(direction, knockbackForce);
        }

        if (pierceRemaining <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            pierceRemaining--;
        }
    }

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    public float GetDamage() => damage;
}
