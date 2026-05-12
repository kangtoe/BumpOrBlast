using UnityEngine;

public class kangtoe99_PlayerShooting : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private kangtoe99_PlayerStats stats;

    [Header("Fallback (stats 없을 때만 사용)")]
    [SerializeField] private float fallbackDamage = 10f;
    [SerializeField] private float fallbackFireRate = 0.35f;
    [SerializeField] private float fallbackBulletSpeed = 20f;
    [SerializeField] private float bulletKnockback = 5f;

    [Header("SFX")]
    [SerializeField] private AudioClip shootSound;

    private float nextFireTime = 0f;

    private void Awake()
    {
        if (stats == null)
        {
            stats = GetComponent<kangtoe99_PlayerStats>();
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        if (kangtoe99_GameManager.Instance != null && !kangtoe99_GameManager.Instance.IsGameStarted()) return;

        float fireRate = stats != null ? stats.GetFinal(kangtoe99_StatType.FireRate) : fallbackFireRate;
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + Mathf.Max(0.05f, fireRate);
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        float damage = stats != null ? stats.GetFinal(kangtoe99_StatType.Damage) : fallbackDamage;
        float bulletSpeed = stats != null ? stats.GetFinal(kangtoe99_StatType.ProjectileSpeed) : fallbackBulletSpeed;
        float bulletScale = stats != null ? stats.GetFinal(kangtoe99_StatType.ProjectileScale) : 1f;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        if (bulletScale != 1f)
        {
            bullet.transform.localScale *= bulletScale;
        }

        kangtoe99_Bullet bulletScript = bullet.GetComponent<kangtoe99_Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(firePoint.up, bulletSpeed, bulletKnockback, damage);
        }

        if (shootSound != null)
        {
            AudioSource.PlayClipAtPoint(shootSound, Camera.main.transform.position);
        }
    }
}
