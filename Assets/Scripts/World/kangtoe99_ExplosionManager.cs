using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

// Explosion 풀 + 좌→우 스윕 폭발 연출 싱글톤(BB2 포팅, 무프리팹 버전).
// 외부 자산 의존성 없음 — Awake에서 절차적 원형 스프라이트 1개를 만들어 모든 폭발 인스턴스가 공유.
// 사용: kangtoe99_ExplosionManager.Instance.SpawnSweep(...) 또는 SpawnOne(...).
// AfterSceneLoad에서 자동 생성(씬 어디에도 배치 불필요), DontDestroyOnLoad.
public class kangtoe99_ExplosionManager : MonoBehaviour
{
    public static kangtoe99_ExplosionManager Instance { get; private set; }

    [Header("Procedural Sprite")]
    [Tooltip("절차 생성될 폭발 스프라이트 한 변(px).")]
    [SerializeField] private int spriteSize = 128;
    [Tooltip("스폰 시 적용할 로컬 스케일. Detonate가 radius×2까지 확장한다.")]
    [SerializeField] private float baseScale = 0.4f;
    [Tooltip("SpriteRenderer.sortingOrder.")]
    [SerializeField] private int sortingOrder = 1;

    private Sprite cachedSprite;
    private ObjectPool<kangtoe99_Explosion> pool;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        if (Instance != null) return;
        var go = new GameObject("[ExplosionManager]");
        go.AddComponent<kangtoe99_ExplosionManager>();
        DontDestroyOnLoad(go);
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        cachedSprite = BuildSolidCircleSprite(spriteSize);

        pool = new ObjectPool<kangtoe99_Explosion>(
            createFunc:      CreateExplosion,
            actionOnGet:     e => e.gameObject.SetActive(true),
            actionOnRelease: e => e.gameObject.SetActive(false),
            actionOnDestroy: e => { if (e != null) Destroy(e.gameObject); },
            collectionCheck: false,
            defaultCapacity: 16,
            maxSize:         128);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        pool?.Clear();
    }

    private kangtoe99_Explosion CreateExplosion()
    {
        var go = new GameObject("Explosion");
        go.transform.localScale = Vector3.one * baseScale;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = cachedSprite;
        sr.color = Color.white;
        sr.sortingOrder = sortingOrder;

        return go.AddComponent<kangtoe99_Explosion>();
    }

    // 솔리드 흰 원 — 반지름 내부는 알파 1, 바깥은 0. 런타임에 RGB tint로 색 입힘.
    private static Sprite BuildSolidCircleSprite(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            alphaIsTransparency = true
        };

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.5f;
        float radiusSq = radius * radius;
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center.x;
                float dy = y - center.y;
                float distSq = dx * dx + dy * dy;
                // 가장자리만 1px 안티앨리어싱(매끈한 원형 윤곽). 내부는 완전 불투명.
                float alpha = distSq <= (radius - 1f) * (radius - 1f) ? 1f
                            : distSq >= radiusSq ? 0f
                            : 1f - (Mathf.Sqrt(distSq) - (radius - 1f));
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply(false, true); // makeNoLongerReadable=true: GPU 업로드 후 CPU 메모리 해제

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    /// <summary>즉시 한 발 터뜨림.</summary>
    public void SpawnOne(Vector2 pos, float damage, float radius, float knockback, Color color)
    {
        if (pool == null) return;
        var e = pool.Get();
        e.Detonate(pos, damage, radius, knockback, color, pool.Release);
    }

    /// <summary>화면 좌→우 sweep. x는 선형 + xJitter(halfW 비율), y는 ±halfH 랜덤.</summary>
    public void SpawnSweep(Vector2 center, float halfW, float halfH, int count, float interval,
        float damage, float radius, float knockback, Color color,
        float xJitterRatio, float yRangeRatio)
    {
        StartCoroutine(SweepRoutine(center, halfW, halfH, count, interval,
            damage, radius, knockback, color, xJitterRatio, yRangeRatio));
    }

    IEnumerator SweepRoutine(Vector2 center, float halfW, float halfH, int count, float interval,
        float damage, float radius, float knockback, Color color,
        float xJitterRatio, float yRangeRatio)
    {
        var wait = new WaitForSeconds(Mathf.Max(0.01f, interval));
        float xJit = Mathf.Max(0f, xJitterRatio) * halfW;
        float yRange = Mathf.Clamp01(yRangeRatio) * halfH;

        for (int i = 0; i < count; i++)
        {
            float tLinear = count > 1 ? i / (count - 1f) : 0.5f;
            float xBase = -halfW + 2f * halfW * tLinear;
            Vector2 pos = center + new Vector2(
                xBase + Random.Range(-xJit, xJit),
                Random.Range(-yRange, yRange));
            SpawnOne(pos, damage, radius, knockback, color);
            if (i < count - 1) yield return wait;
        }
    }
}
