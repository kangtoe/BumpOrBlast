using UnityEngine;

public class kangtoe99_DropBomb : kangtoe99_Drop
{
    // 기본값은 BB2의 ItemData_Bomb.asset 실제 사용값과 동일.
    [Header("Bomb Burst")]
    [Tooltip("연쇄 폭발 횟수.")]
    [SerializeField] private int burstCount = 30;
    [Tooltip("폭발 사이 간격(초).")]
    [SerializeField] private float interval = 0.04f;

    [Header("Each Explosion")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float radius = 3f;
    [Tooltip("BB2 원본은 0 — 화면 가득 폭발이 깔리는 동안 적이 밀려나가지 않게 의도.")]
    [SerializeField] private float knockback = 0f;
    [Tooltip("켜면 explosionColor 사용. 끄면 Drop의 SpriteRenderer 색상을 따른다 — 기본은 끔(렌더러 색 따라가기).")]
    [SerializeField] private bool overrideColor = false;
    [Tooltip("overrideColor가 켜졌을 때만 사용. BB2 원본 값 r=1, g=0.7, b=0.3.")]
    [SerializeField] private Color explosionColor = new Color(1f, 0.7f, 0.3f, 1f);

    [Header("Pattern")]
    [Tooltip("좌→우 선형 위치 위에 x 방향 지터 비율(halfW 곱). 0 = 정확 선형, 0.2 = ±20%.")]
    [Range(0f, 0.5f)]
    [SerializeField] private float xJitterRatio = 0.1f;
    [Tooltip("y 방향 랜덤 범위 비율(halfH 곱). 1 = 화면 전체 세로 랜덤.")]
    [Range(0f, 1f)]
    [SerializeField] private float yRangeRatio = 0.75f;

    protected override void OnPickup(kangtoe99_Player player)
    {
        var cam = Camera.main;
        if (cam == null || kangtoe99_ExplosionManager.Instance == null)
        {
            Debug.LogWarning("[DropBomb] ExplosionManager 또는 Main Camera 없음 — 폭발 생략");
            return;
        }

        float h = cam.orthographicSize;
        float w = h * kangtoe99_AspectRatioController.GetEffectiveAspectRatio(cam);
        Vector2 center = cam.transform.position;

        Color color = overrideColor ? explosionColor : GetRendererColor();
        color.a = 1f;

        kangtoe99_ExplosionManager.Instance.SpawnSweep(
            center, w, h, burstCount, interval,
            damage, radius, knockback, color,
            xJitterRatio, yRangeRatio);
    }

    private Color GetRendererColor()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        return sr != null ? sr.color : explosionColor;
    }
}
