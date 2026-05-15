using System;
using System.Collections;
using UnityEngine;

// 한 번 터지고 풀에 반환되는 일회성 폭발(BB2 포팅).
// Detonate 시 반경 내 모든 kangtoe99_Enemy에 피해 + 중심에서 바깥 방향 임펄스 + scale 확장 / 알파 페이드 VFX.
// VFX 종료 시 releaseCallback 호출(ExplosionManager의 풀 반환).
[RequireComponent(typeof(SpriteRenderer))]
public class kangtoe99_Explosion : MonoBehaviour
{
    const float VfxDuration = 0.25f;
    static readonly Collider2D[] OverlapBuffer = new Collider2D[128];

    SpriteRenderer sr;
    Vector3 baseScale;
    Color baseColor;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        baseColor = sr != null ? sr.color : Color.white;
    }

    public void Detonate(Vector2 pos, float damage, float radius, float knockback,
        Color color, Action<kangtoe99_Explosion> release)
    {
        transform.position = pos;
        transform.localScale = baseScale;
        if (sr != null) sr.color = color;
        baseColor = color;

        // 범위 내 적 — 레이어 마스크 안 씀(BumpOrBlast는 Enemy 전용 레이어가 없고 tag 기반). 컴포넌트로 필터링.
        int n = Physics2D.OverlapCircleNonAlloc(pos, radius, OverlapBuffer);
        for (int i = 0; i < n; i++)
        {
            var c = OverlapBuffer[i];
            if (c == null) continue;
            var enemy = c.GetComponent<kangtoe99_Enemy>();
            if (enemy == null) enemy = c.GetComponentInParent<kangtoe99_Enemy>();
            if (enemy == null) continue;

            Vector2 dir = (Vector2)enemy.transform.position - pos;
            if (dir.sqrMagnitude < 0.0001f) dir = UnityEngine.Random.insideUnitCircle.normalized;
            else dir.Normalize();

            // 임펄스 먼저 — 데미지가 즉시 사망/Destroy를 유발할 수 있으므로 사후 호출은 위험.
            enemy.ApplyKnockback(dir, knockback);
            enemy.TakeDamage(damage, pos);
        }

        StartCoroutine(PlayVfx(radius, release));
    }

    IEnumerator PlayVfx(float radius, Action<kangtoe99_Explosion> release)
    {
        float elapsed = 0f;
        Vector3 endScale = Vector3.one * (radius * 2f);
        while (elapsed < VfxDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / VfxDuration);
            transform.localScale = Vector3.Lerp(baseScale, endScale, t);
            if (sr != null)
                sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, (1f - t) * baseColor.a);
            yield return null;
        }
        release?.Invoke(this);
    }
}
