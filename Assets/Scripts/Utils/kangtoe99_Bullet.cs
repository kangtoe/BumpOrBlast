using UnityEngine;

public class kangtoe99_Bullet : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;
    
    private float damage = 10f;
    private float speed;
    private float knockbackForce;
    private int pierceRemaining;

    private Vector2 direction;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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

        Destroy(gameObject, lifetime);
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
