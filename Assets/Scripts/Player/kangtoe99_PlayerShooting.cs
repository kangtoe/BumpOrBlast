using UnityEngine;

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

    private int currentAmmo;
    private bool isReloading = false;

    private void Start()
    {
        currentAmmo = maxAmmo;
    }

    private void Update()
    {
        if (isReloading)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }

        // TODO: 재장전 기능 (R 키)
        // if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        // {
        //     StartCoroutine(Reload());
        // }
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
    }

    // TODO: 재장전 코루틴
    // private System.Collections.IEnumerator Reload()
    // {
    //     isReloading = true;
    //     Debug.Log("Reloading...");
    //
    //     yield return new WaitForSeconds(reloadTime);
    //
    //     currentAmmo = maxAmmo;
    //     isReloading = false;
    //     Debug.Log("Reload Complete!");
    // }

    public int GetCurrentAmmo() => currentAmmo;
    public int GetMaxAmmo() => maxAmmo;
    public bool IsReloading() => isReloading;

    public void IncreaseMaxAmmo(int amount)
    {
        maxAmmo += amount;
        currentAmmo = maxAmmo;
    }
}
