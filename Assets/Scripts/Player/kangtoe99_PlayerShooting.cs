using UnityEngine;

public class kangtoe99_PlayerShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletDamage = 10f;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float bulletKnockback = 5f;

    [Header("Fire Rate Settings")]
    [SerializeField] private float fireRate = 0.35f;

    [Header("SFX")]
    [SerializeField] private AudioClip shootSound;

    private float nextFireTime = 0f;

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        if (kangtoe99_GameManager.Instance != null && !kangtoe99_GameManager.Instance.IsGameStarted()) return;

        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        kangtoe99_Bullet bulletScript = bullet.GetComponent<kangtoe99_Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(firePoint.up, bulletSpeed, bulletKnockback, bulletDamage);
        }

        if (shootSound != null)
        {
            AudioSource.PlayClipAtPoint(shootSound, Camera.main.transform.position);
        }
    }

    public float GetFireRate() => fireRate;

    public void SetFireRate(float newFireRate)
    {
        fireRate = Mathf.Max(0.05f, newFireRate);
    }

    public float GetBulletDamage() => bulletDamage;

    public void SetBulletDamage(float newDamage)
    {
        bulletDamage = newDamage;
    }

    public float GetBulletKnockback() => bulletKnockback;

    public void SetBulletKnockback(float newKnockback)
    {
        bulletKnockback = Mathf.Max(0f, newKnockback);
    }

    public void IncreaseBulletKnockback(float amount)
    {
        bulletKnockback += amount;
    }
}
