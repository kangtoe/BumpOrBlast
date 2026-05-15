using UnityEngine;

public class kangtoe99_PlayerShooting : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private kangtoe99_PlayerStats stats;
    [SerializeField] private kangtoe99_EnergySystem energy;

    [Header("Fallback (stats 없을 때만 사용)")]
    [SerializeField] private float fallbackDamage = 10f;
    [SerializeField] private float fallbackFireRate = 0.35f;
    [SerializeField] private float fallbackBulletSpeed = 20f;
    [SerializeField] private float fallbackEnergyCost = 1f;
    [SerializeField] private float bulletKnockback = 5f;

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

    private float nextFireTime = 0f;
    private float nextEmptyClickTime = 0f;

    private void Awake()
    {
        if (stats == null)
        {
            stats = GetComponent<kangtoe99_PlayerStats>();
        }
        if (energy == null)
        {
            energy = GetComponent<kangtoe99_EnergySystem>();
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        if (kangtoe99_GameManager.Instance != null && !kangtoe99_GameManager.Instance.IsGameStarted()) return;

        float fireRate = stats != null ? stats.GetFinal(kangtoe99_StatType.FireRate) : fallbackFireRate;
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            float cost = stats != null ? stats.GetFinal(kangtoe99_StatType.EnergyCost) : fallbackEnergyCost;
            if (energy != null && !energy.TryConsume(cost))
            {
                PlayEmptyClick();
                nextFireTime = Time.time + Mathf.Max(0.05f, fireRate);
                return;
            }

            Shoot();
            if (energy != null) energy.ApplyFiringPenalty(fireRate);
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

        float damage = stats != null ? stats.GetFinal(kangtoe99_StatType.Damage) : fallbackDamage;
        float bulletSpeed = stats != null ? stats.GetFinal(kangtoe99_StatType.ProjectileSpeed) : fallbackBulletSpeed;
        float bulletScale = stats != null ? stats.GetFinal(kangtoe99_StatType.ProjectileScale) : 1f;
        int count = stats != null ? Mathf.Max(1, Mathf.RoundToInt(stats.GetFinal(kangtoe99_StatType.ProjectileCount))) : 1;
        float spread = stats != null ? stats.GetFinal(kangtoe99_StatType.ProjectileSpread) : 0f;
        int pierce = stats != null ? Mathf.Max(0, Mathf.RoundToInt(stats.GetFinal(kangtoe99_StatType.Pierce))) : 0;

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
    }
}
