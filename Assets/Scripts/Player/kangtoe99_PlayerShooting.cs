using UnityEngine;
using UnityEngine.UI;

public class kangtoe99_PlayerShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletDamage = 10f;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float bulletKnockback = 5f;

    [Header("Ammo Settings")]
    [SerializeField] private int maxAmmo = 10;
    [SerializeField] private float reloadTime = 2f;

    [Header("Fire Rate Settings")]
    [SerializeField] private float fireRate = 0.2f; // 발사 간격 (초 단위)

    [Header("UI")]
    [SerializeField] private Text ammoText;

    private int currentAmmo;
    private bool isReloading = false;
    private float reloadTimeRemaining = 0f;
    private float nextFireTime = 0f;

    private void Start()
    {
        currentAmmo = maxAmmo;
    }

    private void Update()
    {
        UpdateAmmoUI();

        // 게임 일시 정지 중에는 사격 및 재장전 무시
        if (Time.timeScale == 0f)
            return;

        if (isReloading)
            return;

        // 자동 발사: 마우스 버튼을 누르고 있으면 연사
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();
        }

        // R키로 재장전
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
        }
    }

    private void UpdateAmmoUI()
    {
        if (ammoText == null)
            return;

        if (isReloading)
        {
            ammoText.text = $"Reloading... {reloadTimeRemaining:F1}s";
        }
        else
        {
            ammoText.text = $"{currentAmmo}/{maxAmmo}";
        }
    }

    private void Shoot()
    {
        if (currentAmmo <= 0)
        {
            Debug.Log("No Ammo!");
            return;
        }

        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("BulletPrefab or FirePoint not assigned!");
            return;
        }

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        kangtoe99_Bullet bulletScript = bullet.GetComponent<kangtoe99_Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(firePoint.up, bulletSpeed, bulletKnockback, bulletDamage);
        }

        currentAmmo--;
        nextFireTime = Time.time + fireRate; // 다음 발사 가능 시간 설정

        // 탄창이 비었으면 자동 재장전
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
        }
    }

    private System.Collections.IEnumerator Reload()
    {
        isReloading = true;
        reloadTimeRemaining = reloadTime;
        Debug.Log("Reloading...");

        while (reloadTimeRemaining > 0)
        {
            reloadTimeRemaining -= Time.deltaTime;
            yield return null;
        }

        currentAmmo = maxAmmo;
        isReloading = false;
        reloadTimeRemaining = 0f;
        Debug.Log("Reload Complete!");
    }

    public int GetCurrentAmmo() => currentAmmo;
    public int GetMaxAmmo() => maxAmmo;
    public bool IsReloading() => isReloading;
    public float GetReloadTimeRemaining() => reloadTimeRemaining;

    public void IncreaseMaxAmmo(int amount)
    {
        maxAmmo += amount;
    }

    public float GetBulletDamage() => bulletDamage;

    public void SetBulletDamage(float newDamage)
    {
        bulletDamage = newDamage;
    }

    public float GetReloadTime() => reloadTime;

    public void SetReloadTime(float newReloadTime)
    {
        reloadTime = Mathf.Max(0.1f, newReloadTime); // 최소 0.1초
    }

    public float GetFireRate() => fireRate;

    public void SetFireRate(float newFireRate)
    {
        fireRate = Mathf.Max(0.05f, newFireRate); // 최소 0.05초 (초당 20발)
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
