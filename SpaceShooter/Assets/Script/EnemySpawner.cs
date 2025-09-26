using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnRate = 1f;
    private float nextSpawnTime = 0f;

    public float spawnZ = 20f;

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnRate;
        }
    }

    void SpawnEnemy()
    {
        float halfHeight = Camera.main.orthographicSize;
        float halfWidth = halfHeight * Camera.main.aspect;

        float randomX = UnityEngine.Random.Range(-halfWidth, halfWidth);

        Vector3 spawnPos = new Vector3(randomX, 0f, spawnZ);

        if (enemyPrefab != null)
            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        else
            Debug.LogWarning("EnemySpawner: enemyPrefab is not assigned.");
    }
}
