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

        for (int i = 0; i < count; i++)
        {
            // spread를 count개 슬롯으로 균등 분할 후 각 슬롯 안에서 랜덤 — 샷건 패턴
            float angleOffset = 0f;
            if (spread > 0f)
            {
                float slotSize = spread / count;
                float slotStart = -spread * 0.5f + slotSize * i;
                angleOffset = Random.Range(slotStart, slotStart + slotSize);
            }

            Quaternion rotation = firePoint.rotation * Quaternion.Euler(0f, 0f, angleOffset);
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, rotation);
            if (bulletScale != 1f)
            {
                bullet.transform.localScale *= bulletScale;
            }

            var bulletScript = bullet.GetComponent<kangtoe99_Bullet>();
            if (bulletScript != null)
            {
                Vector2 dir = rotation * Vector3.up;
                bulletScript.Initialize(dir, bulletSpeed, bulletKnockback, damage, pierce);
            }
        }

        if (shootSound != null)
        {
            AudioSource.PlayClipAtPoint(shootSound, Camera.main.transform.position);
        }
    }
}
