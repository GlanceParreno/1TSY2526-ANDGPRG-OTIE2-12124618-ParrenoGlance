using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace TD
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Enemy : MonoBehaviour
    {
        [Header("Stats")]
        public int maxHealth = 30;
        public int damageToCore = 1;
        public int goldReward = 10;
        public bool isBoss = false;
        public bool isFlying = false;

        [Header("Feedback")]
        public ParticleSystem hitVfx;
        public ParticleSystem deathVfx;
        public AudioClip hitSfx;
        public AudioClip deathSfx;

        [Header("UI")]
        public GameObject enemyHealthbarPrefab;
        public Vector3 healthbarOffset = new Vector3(0, 1.6f, 0);


        int currentHealth;
        NavMeshAgent agent;
        Transform coreTarget;
        bool reachedCore = false;


        bool chilled = false;
        float chillTimer = 0f;
        float chillAmount = 0f;

        bool burning = false;
        float burnTimer = 0f;
        int burnDPS = 0;
        float burnTickInterval = 0.2f;
        float burnTickTimer = 0f;

        float baseSpeed = 3.5f;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            currentHealth = maxHealth;
            baseSpeed = agent != null ? agent.speed : 3.5f;
        }

        void Start()
        {

            TD.GameManager.Instance?.RegisterEnemy(this);


            var coreObj = GameObject.FindGameObjectWithTag("Core");
            if (coreObj != null)
            {
                coreTarget = coreObj.transform;
                TrySetDestination(coreTarget.position);
            }


            if (enemyHealthbarPrefab != null)
            {
                var hbInstance = Instantiate(enemyHealthbarPrefab, transform.position + healthbarOffset, Quaternion.identity);
                var comp = hbInstance.GetComponent<EnemyHealthBar>();
                if (comp != null) comp.AttachTo(this);

                hbInstance.transform.SetParent(transform, true);
            }
        }

        void Update()
        {
            HandleDebuffs();

            if (!isFlying && agent != null && coreTarget != null && agent.isOnNavMesh && !reachedCore)
            {
                if (!agent.pathPending && !float.IsPositiveInfinity(agent.remainingDistance))
                {
                    if (agent.remainingDistance <= agent.stoppingDistance + 0.5f)
                    {
                        if (Vector3.Distance(transform.position, coreTarget.position) <= 0.9f)
                        {
                            reachedCore = true;
                            HandleReachCore();
                        }
                    }
                }
            }
        }

        void HandleDebuffs()
        {

            if (chilled)
            {
                chillTimer -= Time.deltaTime;
                if (agent != null)
                    agent.speed = baseSpeed * (1f - chillAmount);

                if (chillTimer <= 0f)
                {
                    chilled = false;
                    chillAmount = 0f;
                    if (agent != null) agent.speed = baseSpeed;
                }
            }


            if (burning)
            {
                burnTimer -= Time.deltaTime;
                burnTickTimer -= Time.deltaTime;
                if (burnTickTimer <= 0f)
                {
                    int dmg = Mathf.Max(1, Mathf.RoundToInt(burnDPS * burnTickInterval));
                    TakeDamage(dmg);
                    burnTickTimer = burnTickInterval;
                }

                if (burnTimer <= 0f)
                {
                    burning = false;
                    burnDPS = 0;
                }
            }
        }


        public void ApplyChill(float percent, float duration)
        {
            chilled = true;
            chillAmount = Mathf.Clamp01(percent);
            chillTimer = Mathf.Max(chillTimer, duration);
            if (agent != null) agent.speed = baseSpeed * (1f - chillAmount);
        }

        public void ApplyBurn(int dps, float duration)
        {
            burning = true;
            burnDPS = Mathf.Max(burnDPS, dps);
            burnTimer = Mathf.Max(burnTimer, duration);
            burnTickTimer = 0f;
        }

        public bool IsChilled() => chilled;
        public bool IsBurning() => burning;
        public int CurrentHealth() => currentHealth;


        public void TakeDamage(int amount)
        {
            if (amount <= 0) return;

            currentHealth -= amount;

            if (hitVfx != null)
            {
                var p = Instantiate(hitVfx, transform.position, Quaternion.identity);
                p.Play();
                Destroy(p.gameObject, 2f);
            }
            if (hitSfx != null)
                AudioSource.PlayClipAtPoint(hitSfx, transform.position);

            if (currentHealth <= 0)
                Die();
        }

        void Die()
        {
            TD.GameManager.Instance?.AddGold(goldReward);

            if (deathVfx != null)
            {
                var p = Instantiate(deathVfx, transform.position, Quaternion.identity);
                p.Play();
                Destroy(p.gameObject, 3f);
            }
            if (deathSfx != null)
                AudioSource.PlayClipAtPoint(deathSfx, transform.position);

            TD.GameManager.Instance?.NotifyEnemyDeath(this);
            Destroy(gameObject);
        }


        void HandleReachCore()
        {
            int dmg = isBoss ? Mathf.Max(damageToCore * 2, 10) : damageToCore;
            TD.GameManager.Instance?.CoreTakeDamage(dmg);
            TD.GameManager.Instance?.NotifyEnemyDeath(this);
            Destroy(gameObject);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Core") && !reachedCore)
            {
                reachedCore = true;
                HandleReachCore();
            }
        }

        public void TrySetDestination(Vector3 dest)
        {
            if (agent == null) return;

            if (!agent.isOnNavMesh)
            {
                if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out var hit, 3f, UnityEngine.AI.NavMesh.AllAreas))
                    agent.Warp(hit.position);
                else
                    Debug.LogWarning("[Enemy] No NavMesh under enemy spawn.");
            }

            if (agent.isOnNavMesh) agent.SetDestination(dest);
        }

        public void ApplyWaveScaling(float hpMultiplier = 1f, float goldMultiplier = 1f)
        {
            if (hpMultiplier <= 0f) hpMultiplier = 1f;
            if (goldMultiplier <= 0f) goldMultiplier = 1f;


            int newMax = Mathf.Max(1, Mathf.RoundToInt(maxHealth * hpMultiplier));
            maxHealth = newMax;
            currentHealth = newMax;


            goldReward = Mathf.Max(0, Mathf.RoundToInt(goldReward * goldMultiplier));
        }
    }
}
