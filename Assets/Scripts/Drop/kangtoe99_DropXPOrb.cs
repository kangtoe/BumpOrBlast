using UnityEngine;

public class kangtoe99_DropXPOrb : kangtoe99_Drop
{
    [Header("XP Orb Settings")]
    [SerializeField] private int scoreValue = 10;

    [Header("Magnet")]
    [Tooltip("Magnet 범위 안에 있을 때 플레이어 쪽으로 가하는 힘.")]
    [SerializeField] private float magnetForce = 5f;
    [Tooltip("이 거리 이내에서는 인력 계산 생략 (떨림 방지).")]
    [SerializeField] private float magnetMinDistance = 0.05f;

    private Transform playerTransform;
    private kangtoe99_PlayerStats playerStats;

    protected override void Start()
    {
        base.Start();

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
    }

    // Magnet 스탯값을 픽업 반경으로 해석 — 반경 안에 들어오면 플레이어 쪽으로 힘을 가한다.
    // 종단 속도 ≈ magnetForce / (mass × drag). drag가 인력을 감쇠시켜 자연스러운 흡입감.
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
