using UnityEngine;

public class kangtoe99_Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 5f;

    private Vector2 direction;
    private float speed;
    private float knockbackForce;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 dir, float bulletSpeed, float knockback, float bulletDamage)
    {
        direction = dir.normalized;
        speed = bulletSpeed;
        knockbackForce = knockback;
        damage = bulletDamage;

        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Bullet hit: {collision.gameObject.name}, Tag: {collision.tag}");

        if (collision.CompareTag("Enemy"))
        {
            kangtoe99_Character enemy = collision.GetComponent<kangtoe99_Character>();
            if (enemy != null)
            {
                Debug.Log($"Enemy hit! Damage: {damage}");
                enemy.TakeDamage(damage);
                enemy.ApplyKnockback(direction, knockbackForce);
            }
            else
            {
                Debug.LogWarning("Enemy tag found but no Character component!");
            }

            Destroy(gameObject);
        }
    }

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    public float GetDamage() => damage;
}
