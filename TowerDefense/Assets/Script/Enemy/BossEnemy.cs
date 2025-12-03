using UnityEngine;

namespace TD
{
    [RequireComponent(typeof(Enemy))]
    public class BossEnemy : MonoBehaviour
    {
        [Header("Boss tuning")]
        public int extraFlatHealth = 200;
        public float healthMultiplier = 2f;
        public int extraGold = 50;
        public float damageToCoreMultiplier = 2f;


        public bool spawnMinions = false;
        public GameObject minionPrefab;
        public int minionsPerSpawn = 2;
        public float minionSpawnInterval = 10f;

        Enemy e;
        float minionTimer = 0f;

        void Awake()
        {
            e = GetComponent<Enemy>();
            if (e == null) return;
            e.isBoss = true;
            e.maxHealth = Mathf.Max(1, Mathf.RoundToInt((e.maxHealth + extraFlatHealth) * healthMultiplier));

            var t = e.GetType();
            var ci = t.GetField("currentHealth", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (ci != null) ci.SetValue(e, e.maxHealth);

            e.goldReward += extraGold;
            e.damageToCore = Mathf.RoundToInt(e.damageToCore * damageToCoreMultiplier);
        }

        void Update()
        {
            if (!spawnMinions || minionPrefab == null) return;
            minionTimer -= Time.deltaTime;
            if (minionTimer <= 0f)
            {
                minionTimer = minionSpawnInterval;
                for (int i = 0; i < minionsPerSpawn; i++)
                {
                    Vector3 pos = transform.position + (Vector3)Random.insideUnitCircle * 1.5f + Vector3.up * 0.2f;
                    Instantiate(minionPrefab, pos, Quaternion.identity);
                }
            }
        }
    }
}
