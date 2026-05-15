using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class kangtoe99_Drop : MonoBehaviour
{
    [Header("Drop Settings")]
    [SerializeField] protected float lifetime = 600f;

    [Header("Physics")]
    [SerializeField] protected float initialForce = 3f;
    [Tooltip("스폰 시 추가되는 전방향 랜덤 임펄스 크기(unit 디스크에 곱). 0 = 사망 벡터만, 1 권장.")]
    [SerializeField] protected float scatterRandomness = 1f;
    [Tooltip("Rigidbody2D.linearDamping을 이 값으로 강제. 0보다 커야 시간 지나면 정지.")]
    [SerializeField] protected float drag = 3f;

    [Header("Repulsion (드롭끼리 너무 가까우면 밀어냄)")]
    [Tooltip("이 거리 내 다른 드롭을 감지해 멀어지는 방향으로 힘을 가한다.")]
    [SerializeField] protected float repulsionRange = 0.7f;
    [Tooltip("최대 반발력(거리 0일 때). 거리가 멀수록 선형 감쇠.")]
    [SerializeField] protected float repulsionForce = 8f;

    [Header("VFX")]
    [SerializeField] protected ParticleSystem pickupParticlePrefab;

    [Header("Pulse Animation")]
    [SerializeField] protected float pulseSpeed = 10f;
    [SerializeField] protected float pulseAmount = 0.05f;

    [Header("Audio")]
    [SerializeField] protected AudioClip pickupSound;

    protected Rigidbody2D rb;
    protected Vector2 dropDirection;
    private Vector3 baseScale;
    private Camera mainCam;
    private static readonly Collider2D[] RepulsionBuffer = new Collider2D[16];

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        baseScale = transform.localScale;
        mainCam = Camera.main;

        if (rb != null)
        {
            rb.linearDamping = drag;

            // 사망 벡터 + 전방향 무작위 임펄스 → 사방으로 퍼졌다 drag로 정지.
            // Random.insideUnitCircle는 균등 분포 unit disk라 각 드롭이 서로 다른 방향으로 흩어진다.
            Vector2 scatter = Random.insideUnitCircle * scatterRandomness;
            Vector2 impulse = (dropDirection + scatter) * initialForce;
            if (impulse.sqrMagnitude > 0.0001f)
            {
                rb.AddForce(impulse, ForceMode2D.Impulse);
            }
        }

        Destroy(gameObject, lifetime);
    }

    protected virtual void Update()
    {
        // 펄스 애니메이션
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = baseScale * pulse;
    }

    protected virtual void FixedUpdate()
    {
        ApplyRepulsion();
    }

    protected virtual void LateUpdate()
    {
        WrapAroundScreen();
    }

    // 가까운 다른 드롭에서 멀어지는 방향으로 힘을 가한다. 거리 0에서 최대, repulsionRange에서 0인 선형.
    // 드롭 콜라이더는 trigger지만 OverlapCircleNonAlloc는 트리거도 잡는다(Queries Hit Triggers 기본 ON).
    private void ApplyRepulsion()
    {
        if (rb == null || repulsionRange <= 0f || repulsionForce <= 0f) return;

        int n = Physics2D.OverlapCircleNonAlloc(transform.position, repulsionRange, RepulsionBuffer);
        for (int i = 0; i < n; i++)
        {
            var c = RepulsionBuffer[i];
            if (c == null || c.gameObject == gameObject) continue;
            // 다른 드롭만 — 플레이어/적은 무시.
            var other = c.GetComponent<kangtoe99_Drop>();
            if (other == null) continue;

            Vector2 away = (Vector2)transform.position - (Vector2)c.transform.position;
            float dist = away.magnitude;
            if (dist < 0.0001f) away = Random.insideUnitCircle.normalized;
            else away /= dist;

            float falloff = 1f - Mathf.Clamp01(dist / repulsionRange);
            rb.AddForce(away * (repulsionForce * falloff));
        }
    }

    // 플레이어와 동일한 wrap-around — 카메라 뷰포트 밖으로 나가면 반대편으로 텔레포트.
    private void WrapAroundScreen()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;

        Vector3 vp = mainCam.WorldToViewportPoint(transform.position);
        bool teleported = false;

        if (vp.x > 1f) { vp.x = 0f; teleported = true; }
        else if (vp.x < 0f) { vp.x = 1f; teleported = true; }

        if (vp.y > 1f) { vp.y = 0f; teleported = true; }
        else if (vp.y < 0f) { vp.y = 1f; teleported = true; }

        if (teleported)
        {
            Vector3 newPos = mainCam.ViewportToWorldPoint(vp);
            newPos.z = transform.position.z;
            transform.position = newPos;
        }
    }

    public void SetDropDirection(Vector2 direction)
    {
        dropDirection = direction.normalized;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out kangtoe99_Player player))
        {
            TriggerPickup(player);
        }
    }

    // 외부(예: InstantDropItemData)에서 충돌 없이 픽업 효과만 즉시 발동시키고 싶을 때 호출.
    public void TriggerPickup(kangtoe99_Player player)
    {
        OnPickup(player);

        if (pickupParticlePrefab != null)
        {
            ParticleSystem vfx = Instantiate(pickupParticlePrefab, transform.position, Quaternion.identity);
            var main = vfx.main;
            main.startColor = GetPickupParticleColor();
            Destroy(vfx.gameObject, 2f);
        }

        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, Camera.main.transform.position);
        }

        Destroy(gameObject);
    }

    // 픽업 파티클 색을 자신의 SpriteRenderer 색에서 가져온다(알파만 1로 고정).
    private Color GetPickupParticleColor()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null) return Color.white;
        Color c = sr.color;
        c.a = 1f;
        return c;
    }

    protected virtual void OnPickup(kangtoe99_Player player) { }
}
