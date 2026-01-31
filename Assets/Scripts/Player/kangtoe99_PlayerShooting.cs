using UnityEngine;
using UnityEngine.UI;

public class kangtoe99_PlayerShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float bulletKnockback = 5f;

    [Header("Ammo Settings")]
    [SerializeField] private int maxAmmo = 10;
    [SerializeField] private float reloadTime = 2f;

    [Header("UI")]
    [SerializeField] private Text ammoText;

    private int currentAmmo;
    private bool isReloading = false;
    private float reloadTimeRemaining = 0f;

    private void Start()
    {
        currentAmmo = maxAmmo;
    }

    private void Update()
    {
        UpdateAmmoUI();

        if (isReloading)
            return;

        if (Input.GetMouseButtonDown(0))
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
            bulletScript.Initialize(firePoint.up, bulletSpeed, bulletKnockback);
        }

        currentAmmo--;

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
        currentAmmo = maxAmmo;
    }
}
