using UnityEngine;

[System.Serializable]
public class Nozzle
{
    public string name;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.2f;
    public int bulletCount = 1;    // for spread/shotgun types
    public float spreadAngle = 15f; // used for spread firing
}
