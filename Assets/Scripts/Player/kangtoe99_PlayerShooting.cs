using UnityEngine;

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

    [Header("Ammo UI Manager")]
    [SerializeField] private kangtoe99_AmmoUIManager ammoUIManager;

    [Header("SFX")]
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip emptyClickSound;
    [SerializeField] private AudioClip reloadStartSound;
    [SerializeField] private AudioClip reloadCompleteSound;

    private int currentAmmo;
    private bool isReloading = false;
    private float reloadTimeRemaining = 0f;
    private float nextFireTime = 0f;

    private void Start()
    {
        currentAmmo = maxAmmo;

        if (ammoUIManager != null)
        {
            ammoUIManager.InitializeAmmoUI(maxAmmo, maxAmmo);
        }
    }

    private void Update()
    {
        // 게임 일시 정지 중에는 사격 및 재장전 무시
        if (Time.timeScale == 0f)
            return;

        if (isReloading)
        {
            // 재장전 중 사격 시도 시 빈 탄창 사운드
            if (Input.GetMouseButtonDown(0) && emptyClickSound != null)
            {
                AudioSource.PlayClipAtPoint(emptyClickSound, Camera.main.transform.position);
            }
            return;
        }

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

    private void Shoot()
    {
        // 탄창이 비었으면 자동 재장전 시작
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
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

        // 발사 사운드 재생
        if (shootSound != null)
        {
            AudioSource.PlayClipAtPoint(shootSound, Camera.main.transform.position);
        }

        currentAmmo--;
        nextFireTime = Time.time + fireRate; // 다음 발사 가능 시간 설정

        if (ammoUIManager != null)
        {
            ammoUIManager.OnShoot(firePoint.position);
        }
    }

    private System.Collections.IEnumerator Reload()
    {
        isReloading = true;
        reloadTimeRemaining = reloadTime;

        // 재장전 시작 사운드
        if (reloadStartSound != null)
        {
            AudioSource.PlayClipAtPoint(reloadStartSound, Camera.main.transform.position);
        }

        if (ammoUIManager != null)
        {
            ammoUIManager.OnReloadStart(reloadTime);
        }

        while (reloadTimeRemaining > 0)
        {
            reloadTimeRemaining -= Time.deltaTime;
            yield return null;
        }

        currentAmmo = maxAmmo;
        isReloading = false;
        reloadTimeRemaining = 0f;

        // 재장전 완료 사운드
        if (reloadCompleteSound != null)
        {
            AudioSource.PlayClipAtPoint(reloadCompleteSound, Camera.main.transform.position);
        }
    }

    public int GetCurrentAmmo() => currentAmmo;
    public int GetMaxAmmo() => maxAmmo;
    public bool IsReloading() => isReloading;
    public float GetReloadTimeRemaining() => reloadTimeRemaining;

    public void IncreaseMaxAmmo(int amount)
    {
        maxAmmo += amount;

        if (ammoUIManager != null)
        {
            ammoUIManager.InitializeAmmoUI(currentAmmo, maxAmmo);
        }
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
