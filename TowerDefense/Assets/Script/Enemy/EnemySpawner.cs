using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD
{
    [System.Serializable]
    public class WaveEntry
    {
        [Tooltip("Enemy prefab to spawn")]
        public GameObject enemyPrefab;
        [Tooltip("How many of this prefab to spawn in this wave")]
        public int count = 5;
        [Tooltip("Delay between each spawn of this entry (seconds)")]
        public float spawnInterval = 0.5f;
        [Tooltip("Optional delay before this entry starts (seconds)")]
        public float startDelay = 0f;

        [Header("Special entry flags")]
        [Tooltip("If true, this entry uses a boss prefab or will be treated as a boss.")]
        public bool isBoss = false;
    }

    [System.Serializable]
    public class Wave
    {
        public string waveName = "Wave";
        public List<WaveEntry> entries = new List<WaveEntry>();
        public float delayBeforeNextWave = 5f;
        public bool spawnEntriesSequentially = false;
    }

    [DisallowMultipleComponent]
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn points (multiple allowed)")]
        public Transform[] spawnPoints;

        [Header("Waves (define composition per wave)")]
        public List<Wave> waves = new List<Wave>();

        [Header("Scaling (applied per wave index)")]
        public float hpScalePerWave = 0.1f;
        public float goldScalePerWave = 0.05f;

        [Header("Auto progression")]
        public bool autoStartNextWave = true;
        public bool autoStartFirstWave = false;
        public int startWaveIndex = 0;

        int currentWaveIndex = -1;
        bool isWaveRunning = false;

        void Start()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
                Debug.LogWarning("[EnemySpawner] No spawnPoints assigned. Spawner will skip spawning if none are present.");

            if (autoStartFirstWave && waves != null && waves.Count > 0)
            {
                StartWave(startWaveIndex);
            }
        }




        public void StartNextWave()
        {
            StartWave(currentWaveIndex + 1);
        }




        public void StartWave(int index)
        {
            if (isWaveRunning)
            {
                Debug.LogWarning("[EnemySpawner] StartWave called but a wave is already running.");
                return;
            }

            if (waves == null || waves.Count == 0)
            {
                Debug.LogWarning("[EnemySpawner] No waves configured on this spawner.");
                return;
            }

            if (index < 0)
            {
                Debug.LogWarning($"[EnemySpawner] StartWave called with negative index {index} — ignoring.");
                return;
            }

            if (index >= waves.Count)
            {
                Debug.Log($"[EnemySpawner] All waves complete or index {index} out of range (waves.Count={waves.Count}). No more waves to start.");
                return;
            }

            currentWaveIndex = index;
            StartCoroutine(RunWaveCoroutine(waves[index]));
        }

        IEnumerator RunWaveCoroutine(Wave wave)
        {
            isWaveRunning = true;
            Debug.Log($"[EnemySpawner] >>> STARTING wave #{currentWaveIndex + 1}: {wave.waveName} (entries={wave.entries.Count})");


            if (TD.GameManager.Instance != null)
            {
                TD.GameManager.Instance.StartNextWave();
            }

            if (wave.spawnEntriesSequentially)
            {
                foreach (var entry in wave.entries)
                {
                    yield return StartCoroutine(SpawnEntryCoroutine(entry));
                }
            }
            else
            {
                List<Coroutine> running = new List<Coroutine>();
                foreach (var entry in wave.entries)
                    running.Add(StartCoroutine(SpawnEntryCoroutine(entry)));
                foreach (var c in running) yield return c;
            }

            Debug.Log($"[EnemySpawner] All spawns for wave #{currentWaveIndex + 1} done.");


            while (FindObjectsOfType<TD.Enemy>().Length > 0)
            {
                yield return null;
            }

            Debug.Log($"[EnemySpawner] Wave #{currentWaveIndex + 1} cleared.");

            isWaveRunning = false;


            if (autoStartNextWave)
            {

                int nextIndex = currentWaveIndex + 1;
                if (nextIndex >= waves.Count)
                {
                    Debug.Log("[EnemySpawner] Completed last wave. No further waves to start.");
                    yield break;
                }

                if (wave.delayBeforeNextWave > 0f)
                {
                    Debug.Log($"[EnemySpawner] Waiting {wave.delayBeforeNextWave}s before starting wave #{nextIndex + 1}.");
                    yield return new WaitForSeconds(wave.delayBeforeNextWave);
                }
                else
                {

                    yield return null;
                }


                if (!isWaveRunning)
                    StartWave(nextIndex);
                else
                    Debug.LogWarning("[EnemySpawner] Unexpected: tried to start next wave but spawner still marked as running.");
            }
            else
            {
                Debug.Log("[EnemySpawner] autoStartNextWave is false — waiting for explicit StartNextWave() call.");
            }
        }

        IEnumerator SpawnEntryCoroutine(WaveEntry entry)
        {
            if (entry == null)
            {
                Debug.LogWarning("[EnemySpawner] SpawnEntry is null — skipping.");
                yield break;
            }

            if (entry.enemyPrefab == null)
            {
                Debug.LogWarning("[EnemySpawner] WaveEntry has null enemyPrefab — skipping this entry.");
                yield break;
            }

            if (entry.count <= 0)
            {
                Debug.LogWarning("[EnemySpawner] WaveEntry count <= 0 — nothing to spawn.");
                yield break;
            }

            if (entry.startDelay > 0f)
                yield return new WaitForSeconds(entry.startDelay);

            for (int i = 0; i < entry.count; i++)
            {
                Transform spawnPoint = ChooseSpawnPoint();
                if (spawnPoint == null)
                {
                    Debug.LogWarning("[EnemySpawner] No spawn point available; skipping this spawn.");
                }
                else
                {
                    var inst = Instantiate(entry.enemyPrefab, spawnPoint.position, spawnPoint.rotation);

                    float hpMult = 1f + (currentWaveIndex * hpScalePerWave);
                    float goldMult = 1f + (currentWaveIndex * goldScalePerWave);

                    var ec = inst.GetComponent<TD.Enemy>();
                    if (ec != null) ec.ApplyWaveScaling(hpMult, goldMult);

                    if (entry.isBoss && inst.GetComponent<BossEnemy>() == null)
                        inst.AddComponent<BossEnemy>();
                }

                if (entry.spawnInterval > 0f)
                    yield return new WaitForSeconds(entry.spawnInterval);
                else
                    yield return null;
            }
        }

        Transform ChooseSpawnPoint()
        {
            if (spawnPoints == null || spawnPoints.Length == 0) return null;
            if (spawnPoints.Length == 1) return spawnPoints[0];
            return spawnPoints[Random.Range(0, spawnPoints.Length)];
        }

        [ContextMenu("Test Start Next Wave")]
        void EditorTestStartNext() => StartNextWave();
    }
}
