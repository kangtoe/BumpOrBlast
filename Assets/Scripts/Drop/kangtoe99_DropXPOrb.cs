using UnityEngine;

public class kangtoe99_DropXPOrb : kangtoe99_Drop
{
    [Header("XP Orb Settings")]
    [SerializeField] private int scoreValue = 10;
    [Tooltip("XP orb 전용 lifetime(초). base의 lifetime을 덮어쓴다 — 일반 드롭과 달리 짧게 두고 깜빡이며 사라진다.")]
    [SerializeField] private float xpLifetime = 20f;

    [Header("Magnet")]
    [Tooltip("Magnet 범위 안에 있을 때 플레이어 쪽으로 가하는 힘.")]
    [SerializeField] private float magnetForce = 30f;
    [Tooltip("이 거리 이내에서는 인력 계산 생략 (떨림 방지).")]
    [SerializeField] private float magnetMinDistance = 0.05f;

    [Header("Despawn Blink")]
    [Tooltip("사라지기 몇 초 전부터 깜빡일지.")]
    [SerializeField] private float blinkStartBeforeEnd = 5f;
    [Tooltip("깜빡임 주파수(Hz).")]
    [SerializeField] private float blinkSpeed = 8f;
    [Tooltip("깜빡임 시 최저 알파.")]
    [SerializeField, Range(0f, 1f)] private float blinkMinAlpha = 0.2f;

    private float spawnTime;
    private SpriteRenderer cachedRenderer;
    private float baseAlpha = 1f;
    private Transform playerTransform;
    private kangtoe99_PlayerStats playerStats;

    protected override void Start()
    {
        // base.Start가 Destroy(gameObject, lifetime)을 등록하므로 그 전에 덮어쓴다.
        lifetime = xpLifetime;
        base.Start();

        spawnTime = Time.time;

        cachedRenderer = GetComponent<SpriteRenderer>();
        if (cachedRenderer == null) cachedRenderer = GetComponentInChildren<SpriteRenderer>();
        if (cachedRenderer != null) baseAlpha = cachedRenderer.color.a;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerStats = playerObj.GetComponent<kangtoe99_PlayerStats>();
        }
    }

    protected override void Update()
    {
        base.Update();
        ApplyMagnet();
        ApplyDespawnBlink();
    }

    // Magnet 스탯값을 픽업 반경으로 해석 — 반경 안에 들어오면 플레이어 쪽으로 힘을 가한다.
    // 평형 속도는 Rigidbody2D.linearDamping이 알아서 잡고, 가까울수록 정지 직전이라 자연스러운 흡입감.
    private void ApplyMagnet()
    {
        if (rb == null || playerTransform == null || playerStats == null) return;
        float magnetRange = playerStats.GetFinal(kangtoe99_StatType.Magnet);
        if (magnetRange <= 0f) return;

        Vector2 toPlayer = (Vector2)playerTransform.position - (Vector2)transform.position;
        float dist = toPlayer.magnitude;
        if (dist > magnetRange || dist < magnetMinDistance) return;

        rb.AddForce(toPlayer / dist * magnetForce);
    }

    // 사라지기 blinkStartBeforeEnd초 전부터 사인파로 알파를 흔든다.
    private void ApplyDespawnBlink()
    {
        if (cachedRenderer == null) return;

        float remaining = lifetime - (Time.time - spawnTime);
        if (remaining > blinkStartBeforeEnd)
        {
            // 깜빡임 구간 밖에서는 원래 알파 유지 (이전 흔들림 잔재 복구).
            Color restore = cachedRenderer.color;
            if (!Mathf.Approximately(restore.a, baseAlpha))
            {
                restore.a = baseAlpha;
                cachedRenderer.color = restore;
            }
            return;
        }

        float wave = 0.5f * (Mathf.Sin(Time.time * blinkSpeed * Mathf.PI * 2f) + 1f);
        float alpha = Mathf.Lerp(blinkMinAlpha, baseAlpha, wave);
        Color c = cachedRenderer.color;
        c.a = alpha;
        cachedRenderer.color = c;
    }

    protected override void OnPickup(kangtoe99_Player player)
    {
        if (kangtoe99_ScoreSystem.Instance != null)
        {
            kangtoe99_ScoreSystem.Instance.AddScore(scoreValue);
        }
        Debug.Log($"XP Orb picked up! +{scoreValue} score");
    }

    public void SetScoreValue(int value)
    {
        scoreValue = value;
    }
}
