using System.Collections;
using UnityEngine;

[RequireComponent(typeof(kangtoe99_PlayerStats), typeof(kangtoe99_EnergySystem))]
public class kangtoe99_PlayerShooting : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Combat")]
    [SerializeField] private float bulletKnockback = 5f;

    [Header("Recoil Squash VFX")]
    [Tooltip("스쿼시 대상 Transform. 비워두면 자식 SpriteRenderer를 자동 사용 — 루트가 아니므로 콜라이더에 영향 없음")]
    [SerializeField] private Transform squashTarget;
    [Tooltip("반동 squash 피크 — 옆(X)으로 부풀고 앞뒤(Y)로 눌림")]
    [SerializeField] private Vector2 squashScale = new Vector2(1.2f, 0.85f);
    [SerializeField] private float squashDuration = 0.06f;
    [Tooltip("원복 단계 지속 시간 — 끝에 탄성 overshoot")]
    [SerializeField] private float settleDuration = 0.12f;
    [Tooltip("원복 단계 overshoot 강도. 0이면 단순 ease-out")]
    [SerializeField, Range(0f, 0.3f)] private float settleOvershoot = 0.12f;

    [Header("Multi-shot Formation (Count >= 2)")]
    [Tooltip("발사체 사이 간격의 고정 오프셋(유닛). 스케일과 무관")]
    [SerializeField] private float formationSideSpacing = 0.6f;
    [Tooltip("발사체 스케일에 비례해 추가되는 간격(유닛 × bulletScale). 큰 발사체는 자동으로 더 벌어짐. 0이면 비활성")]
    [SerializeField] private float formationSideScalePadding = 0f;
    [Tooltip("멀티샷 시 양 끝 발사체의 속도 배율 (가운데=1.0). 외곽일수록 느려짐")]
    [SerializeField, Range(0.1f, 1f)] private float formationOuterSpeedMultiplier = 0.7f;

    [Header("SFX")]
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip emptyClickSound;
    [SerializeField] private float emptyClickCooldown = 0.3f;

    private kangtoe99_PlayerStats stats;
    private kangtoe99_EnergySystem energy;
    private float nextFireTime = 0f;
    private float nextEmptyClickTime = 0f;

    private Coroutine recoilRoutine;
    private Vector3 squashBaseScale = Vector3.one;

    private void Awake()
    {
        stats = GetComponent<kangtoe99_PlayerStats>();
        energy = GetComponent<kangtoe99_EnergySystem>();

        if (squashTarget == null)
        {
            var sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null) squashTarget = sr.transform;
        }
        if (squashTarget != null) squashBaseScale = squashTarget.localScale;
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        // 시작 전엔 에너지 무제한 — 소모와 사격 패널티를 건너뛰어 자유롭게 조작감을 익히게 한다.
        bool started = kangtoe99_GameManager.Instance == null || kangtoe99_GameManager.Instance.IsGameStarted();

        float fireRate = stats.GetFinal(kangtoe99_StatType.FireRate);
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            if (started)
            {
                float cost = stats.GetFinal(kangtoe99_StatType.EnergyCost);
                if (!energy.TryConsume(cost))
                {
                    PlayEmptyClick();
                    nextFireTime = Time.time + Mathf.Max(0.05f, fireRate);
                    return;
                }
            }

            Shoot();
            if (started) energy.ApplyFiringPenalty(fireRate);
            nextFireTime = Time.time + Mathf.Max(0.05f, fireRate);
        }
    }

    private void PlayEmptyClick()
    {
        if (emptyClickSound == null) return;
        if (Time.time < nextEmptyClickTime) return;
        AudioSource.PlayClipAtPoint(emptyClickSound, Camera.main.transform.position);
        nextEmptyClickTime = Time.time + emptyClickCooldown;
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        float damage = stats.GetFinal(kangtoe99_StatType.Damage);
        float bulletSpeed = stats.GetFinal(kangtoe99_StatType.ProjectileSpeed);
        float bulletScale = stats.GetFinal(kangtoe99_StatType.ProjectileScale);
        int count = Mathf.Max(1, Mathf.RoundToInt(stats.GetFinal(kangtoe99_StatType.ProjectileCount)));
        float spread = stats.GetFinal(kangtoe99_StatType.ProjectileSpread);
        int pierce = Mathf.Max(0, Mathf.RoundToInt(stats.GetFinal(kangtoe99_StatType.Pierce)));

        // 멀티샷 인접 탄환 간격 = 고정 오프셋 + 스케일 비례 패딩
        float effectiveSideSpacing = formationSideSpacing + formationSideScalePadding * bulletScale;

        for (int i = 0; i < count; i++)
        {
            // 각도 슬롯은 위치 i의 반대편을 사용 — 양 끝 위치가 가운데를 향하도록 교차.
            // i=0(가장 왼쪽 위치) → 가장 오른쪽 각도 슬롯, i=count-1(가장 오른쪽 위치) → 가장 왼쪽 각도 슬롯.
            float angleOffset = 0f;
            if (spread > 0f)
            {
                float slotSize = spread / count;
                int angleSlot = (count - 1) - i;
                float slotStart = -spread * 0.5f + slotSize * angleSlot;
                angleOffset = Random.Range(slotStart, slotStart + slotSize);
            }

            // Count >= 2일 때 좌우로 펼쳐진 일자 시작 위치 + 외곽 속도 감소.
            // 사이드: effectiveSideSpacing × 가운데 기준 인덱스 오프셋 — count 늘어도 간격 유지.
            // 속도: |정규화 위치|×2 (= 0~1)로 1 → formationOuterSpeedMultiplier 사이를 lerp.
            Vector3 spawnPos = firePoint.position;
            float thisBulletSpeed = bulletSpeed;
            if (count > 1)
            {
                float t = ((float)i / (count - 1)) - 0.5f; // -0.5 ~ +0.5 (정규화 위치)
                float sideOffset = (i - (count - 1) * 0.5f) * effectiveSideSpacing;
                spawnPos += firePoint.right * sideOffset;

                thisBulletSpeed = bulletSpeed * Mathf.Lerp(1f, formationOuterSpeedMultiplier, Mathf.Abs(t) * 2f);
            }

            Quaternion rotation = firePoint.rotation * Quaternion.Euler(0f, 0f, angleOffset);
            GameObject bullet = Instantiate(bulletPrefab, spawnPos, rotation);
            if (bulletScale != 1f)
            {
                bullet.transform.localScale *= bulletScale;
            }

            var bulletScript = bullet.GetComponent<kangtoe99_Bullet>();
            if (bulletScript != null)
            {
                Vector2 dir = rotation * Vector3.up;
                bulletScript.Initialize(dir, thisBulletSpeed, bulletKnockback, damage, pierce);
            }
        }

        if (shootSound != null)
        {
            AudioSource.PlayClipAtPoint(shootSound, Camera.main.transform.position);
        }

        PlayRecoilSquash();
    }

    private void PlayRecoilSquash()
    {
        if (squashTarget == null) return;
        if (recoilRoutine != null) StopCoroutine(recoilRoutine);
        recoilRoutine = StartCoroutine(RecoilSquashRoutine());
    }

    private IEnumerator RecoilSquashRoutine()
    {
        Vector3 squash = Vector3.Scale(squashBaseScale, new Vector3(squashScale.x, squashScale.y, 1f));
        Vector3 start = squashTarget.localScale; // 연사 시 현재 위치에서 이어서 보간

        yield return TweenScale(start, squash, squashDuration, EaseOutQuad);
        yield return TweenScale(squash, squashBaseScale, settleDuration, EaseOutBackWithOvershoot);

        squashTarget.localScale = squashBaseScale;
        recoilRoutine = null;
    }

    private IEnumerator TweenScale(Vector3 from, Vector3 to, float duration, System.Func<float, float> ease)
    {
        if (duration <= 0f) { squashTarget.localScale = to; yield break; }
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            squashTarget.localScale = Vector3.LerpUnclamped(from, to, ease(k));
            yield return null;
        }
        squashTarget.localScale = to;
    }

    private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);

    private float EaseOutBackWithOvershoot(float t)
    {
        // c1 튜닝: settleOvershoot=0.1일 때 표준 ease-out-back(c1≈1.70158) 강도.
        float c1 = 17.0158f * settleOvershoot;
        float c3 = c1 + 1f;
        float u = t - 1f;
        return 1f + c3 * u * u * u + c1 * u * u;
    }
}
