using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class kangtoe99_Item : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] protected float lifetime = 600f;

    [Header("Physics")]
    [SerializeField] protected float initialForce = 1f;

    [Header("VFX")]
    [SerializeField] protected ParticleSystem pickupParticlePrefab;
    [SerializeField] protected Color pickupColor = Color.white;

    [Header("Pulse Animation")]
    [SerializeField] protected float pulseSpeed = 10f;
    [SerializeField] protected float pulseAmount = 0.05f;

    [Header("Audio")]
    [SerializeField] protected AudioClip pickupSound;

    protected Rigidbody2D rb;
    private Vector2 dropDirection;
    private Vector3 baseScale;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        baseScale = transform.localScale;

        if (rb != null && dropDirection != Vector2.zero)
        {
            rb.AddForce(dropDirection * initialForce, ForceMode2D.Impulse);
        }

        Destroy(gameObject, lifetime);
    }

    protected virtual void Update()
    {
        // 펄스 애니메이션
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = baseScale * pulse;
    }

    public void SetDropDirection(Vector2 direction)
    {
        dropDirection = direction.normalized;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out kangtoe99_Player player))
        {
            OnPickup(player);

            // 파티클 효과
            if (pickupParticlePrefab != null)
            {
                ParticleSystem vfx = Instantiate(pickupParticlePrefab, transform.position, Quaternion.identity);
                var main = vfx.main;
                main.startColor = pickupColor;
                Destroy(vfx.gameObject, 2f);
            }

            // 사운드
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, Camera.main.transform.position);
            }

            Destroy(gameObject);
        }
    }

    protected virtual void OnPickup(kangtoe99_Player player) { }
}
