using UnityEngine;

namespace TD.Towers
{
    [RequireComponent(typeof(TowerData))]
    [RequireComponent(typeof(TowerTargeting))]
    public class TowerShooter : MonoBehaviour
    {
        TowerData data;
        TowerTargeting targeting;

        float cooldown = 0f;

        [Header("Shoot Control")]
        public bool enableShooting = true;

        [Header("Debug")]
        public bool debugLogs = false;
        public bool drawGizmos = true;

        TD.Enemy lastShotTarget;

        void Awake()
        {
            data = GetComponent<TowerData>();
            targeting = GetComponent<TowerTargeting>();
            cooldown = 0f;
        }

        void Update()
        {
            if (data == null || targeting == null) return;

            cooldown -= Time.deltaTime;

            if (cooldown <= 0f && targeting.HasTargets())
            {
                var target = targeting.GetBestTarget();
                if (target != null)
                {
                    if (debugLogs)
                        Debug.Log($"[Shooter] {name} SHOOTING at {target.name}");

                    lastShotTarget = target;
                    FireAt(target);

                    cooldown = Mathf.Max(0.001f, data.fireRate);
                }
            }
        }

        void FireAt(TD.Enemy target)
        {
            if (target == null) return;

            int rolled = data.RollDamage();


            if (data.projectilePrefab == null || data.fireOrigin == null)
            {
                if (debugLogs)
                    Debug.Log($"[Shooter] {name} INSTANT HIT -> {rolled}");

                InstantHit(target, rolled);
                return;
            }


            var go = Instantiate(data.projectilePrefab, data.fireOrigin.position, Quaternion.identity);
            var proj = go.GetComponent<Projectile>();

            if (debugLogs)
                Debug.Log($"[Shooter] {name} SPAWNED PROJECTILE");

            if (proj != null)
            {
                proj.Initialize(
                    target,
                    rolled,
                    data.appliesBurn, data.burnDPS, data.burnDuration,
                    data.appliesChill, data.chillAmount, data.chillDuration,
                    data.hasSplash, data.splashRadius, data.splashDamage,
                    false
                );
            }
            else
            {
                InstantHit(target, rolled);
                Destroy(go);
            }
        }

        void InstantHit(TD.Enemy target, int dmg)
        {
            if (data.hasSplash)
            {
                SplashDamage.ApplySplash(
                    target.transform.position,
                    data.splashRadius,
                    data.splashDamage,
                    data.appliesBurn,
                    data.burnDPS, data.burnDuration,
                    data.appliesChill,
                    data.chillAmount, data.chillDuration
                );
            }
            else
            {
                target.TakeDamage(dmg);
                if (data.appliesBurn) target.ApplyBurn(data.burnDPS, data.burnDuration);
                if (data.appliesChill) target.ApplyChill(data.chillAmount, data.chillDuration);
            }
        }




        void OnDrawGizmos()
        {
            if (!drawGizmos) return;
            if (data == null) return;


            if (data.fireOrigin != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(data.fireOrigin.position, 0.1f);
            }


            if (lastShotTarget != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(data.fireOrigin != null ? data.fireOrigin.position : transform.position,
                                lastShotTarget.transform.position);


                if (data.hasSplash)
                {
                    Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
                    Gizmos.DrawWireSphere(lastShotTarget.transform.position, data.splashRadius);
                }
            }
        }

        [ContextMenu("DEBUG FIRE")]
        public void ForceFireOnce()
        {
            var t = targeting.GetBestTarget();
            if (t != null)
            {
                if (debugLogs)
                    Debug.Log($"[Shooter] {name} FORCE FIRE at {t.name}");

                lastShotTarget = t;
                FireAt(t);
            }
        }
    }
}
