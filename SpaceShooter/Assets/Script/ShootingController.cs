using UnityEngine;

public class ShootingController : MonoBehaviour
{
    [Header("Weapon Setup")]
    public Nozzle[] nozzles;       // assign in Inspector
    private int currentNozzleIndex = 0;

    private float nextFireTime = 0f;

    void Update()
    {
        // Handle nozzle switching (keys 1–4)
        if (Input.GetKeyDown(KeyCode.Alpha1)) currentNozzleIndex = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2) && nozzles.Length > 1) currentNozzleIndex = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3) && nozzles.Length > 2) currentNozzleIndex = 2;
        if (Input.GetKeyDown(KeyCode.Alpha4) && nozzles.Length > 3) currentNozzleIndex = 3;

        // Shooting with current nozzle
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        Nozzle nozzle = nozzles[currentNozzleIndex];

        if (nozzle.bulletPrefab == null || nozzle.firePoint == null)
        {
            Debug.LogWarning("Nozzle not set up correctly!");
            return;
        }

        if (nozzle.bulletCount == 1)
        {
            // Single bullet
            Instantiate(nozzle.bulletPrefab, nozzle.firePoint.position, nozzle.firePoint.rotation);
        }
        else
        {
            // Spread fire (multiple bullets)
            float startAngle = -nozzle.spreadAngle * (nozzle.bulletCount - 1) / 2f;

            for (int i = 0; i < nozzle.bulletCount; i++)
            {
                float angle = startAngle + nozzle.spreadAngle * i;
                Quaternion rotation = nozzle.firePoint.rotation * Quaternion.Euler(0, angle, 0);
                Instantiate(nozzle.bulletPrefab, nozzle.firePoint.position, rotation);
            }
        }

        nextFireTime = Time.time + nozzle.fireRate;
    }
}
