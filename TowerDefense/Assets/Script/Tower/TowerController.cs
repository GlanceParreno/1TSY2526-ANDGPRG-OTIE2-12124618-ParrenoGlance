using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TD.Towers
{
    [RequireComponent(typeof(TowerData))]
    public class TowerController : MonoBehaviour
    {
        TowerData data;
        float cooldown = 0f;
        readonly List<TD.Enemy> nearbyEnemies = new List<TD.Enemy>();


        public TowerUpgrade upgrade;
        [Tooltip("Current upgrade level (0 = base).")]
        public int currentLevel = 0;

        void Awake()
        {
            data = GetComponent<TowerData>();
            cooldown = 0f;

            upgrade = GetComponent<TowerUpgrade>();

            var sc = GetComponent<SphereCollider>();
            if (sc == null)
            {
                sc = gameObject.AddComponent<SphereCollider>();
                sc.isTrigger = true;
                sc.radius = GetCurrentRange();
            }
            else
            {
                sc.isTrigger = true;
                sc.radius = GetCurrentRange();
            }
        }

        void Update()
        {
            if (cooldown > 0f) cooldown -= Time.deltaTime;

            var sc = GetComponent<SphereCollider>();
            if (sc != null && data != null && Mathf.Abs(sc.radius - GetCurrentRange()) > 0.01f)
                sc.radius = GetCurrentRange();

            if (cooldown <= 0f)
            {
                var target = AcquireTarget();
                if (target != null)
                {
                    FireAt(target);
                    cooldown = Mathf.Max(0.001f, GetCurrentFireRate());
                }
            }
        }




        float GetCurrentRange()
        {
            if (upgrade != null) return upgrade.GetLevel(currentLevel).range;
            return data != null ? data.range : 3f;
        }

        float GetCurrentFireRate()
        {
            if (upgrade != null) return upgrade.GetLevel(currentLevel).fireRate;
            return data != null ? data.fireRate : 1f;
        }

        int RollDamage()
        {
            if (upgrade != null)
            {
                var lvl = upgrade.GetLevel(currentLevel);
                int min = Mathf.Max(0, Mathf.Min(lvl.minDamage, lvl.maxDamage));
                int max = Mathf.Max(min, lvl.maxDamage);
                return Random.Range(min, max + 1);
            }


            return data != null ? data.RollDamage() : 0;
        }


        public int GetNextUpgradeCost()
        {
            if (upgrade == null) return -1;
            return upgrade.GetNextLevelCost(currentLevel);
        }


        public bool TryUpgrade()
        {
            if (upgrade == null)
            {
                Debug.Log("[TowerController] No TowerUpgrade attached to this tower.");
                return false;
            }

            int cost = upgrade.GetNextLevelCost(currentLevel);
            if (cost < 0)
            {
                Debug.Log("[TowerController] Already at max level or no next level.");
                return false;
            }

            if (!TD.GameManager.Instance.SpendGold(cost))
            {
                Debug.Log("[TowerController] Not enough gold to upgrade.");
                return false;
            }

            currentLevel++;

            var sc = GetComponent<SphereCollider>();
            if (sc != null) sc.radius = GetCurrentRange();

            Debug.Log($"[TowerController] Upgraded {name} to level {currentLevel}.");
            return true;
        }




        TD.Enemy AcquireTarget()
        {
            nearbyEnemies.RemoveAll(e => e == null);

            if (nearbyEnemies.Count == 0) return null;

            var valid = nearbyEnemies.Where(e =>
                e != null &&
                e.CurrentHealth() > 0 &&
                ((e.isFlying && (upgrade != null ? upgrade.GetLevel(currentLevel).hasSplash : data.canTargetFlying)) || (!e.isFlying && (upgrade != null ? upgrade.GetLevel(currentLevel).hasSplash : data.canTargetGround)))
            ).ToList();

            if (valid.Count == 0) return null;



            bool checkChill = upgrade != null ? upgrade.GetLevel(currentLevel).appliesChill : data.appliesChill;
            bool checkBurn = upgrade != null ? upgrade.GetLevel(currentLevel).appliesBurn : data.appliesBurn;

            if (checkChill)
            {
                var chilled = valid.Where(e => e.IsChilled()).ToList();
                if (chilled.Count > 0) valid = chilled;
            }
            if (checkBurn)
            {
                var burning = valid.Where(e => e.IsBurning()).ToList();
                if (burning.Count > 0) valid = burning;
            }

            TD.Enemy best = null;
            float bestDist = float.MaxValue;
            foreach (var e in valid)
            {
                float d = Vector3.Distance(transform.position, e.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = e;
                }
            }

            return best;
        }

        void FireAt(TD.Enemy target)
        {
            if (target == null || data == null) return;

            int rolledDamage = RollDamage();


            bool curAppliesBurn = upgrade != null ? upgrade.GetLevel(currentLevel).appliesBurn : data.appliesBurn;
            int curBurnDPS = upgrade != null ? upgrade.GetLevel(currentLevel).burnDPS : data.burnDPS;
            float curBurnDur = upgrade != null ? upgrade.GetLevel(currentLevel).burnDuration : data.burnDuration;

            bool curAppliesChill = upgrade != null ? upgrade.GetLevel(currentLevel).appliesChill : data.appliesChill;
            float curChillAmount = upgrade != null ? upgrade.GetLevel(currentLevel).chillAmount : data.chillAmount;
            float curChillDur = upgrade != null ? upgrade.GetLevel(currentLevel).chillDuration : data.chillDuration;

            bool curHasSplash = upgrade != null ? upgrade.GetLevel(currentLevel).hasSplash : data.hasSplash;
            float curSplashRadius = upgrade != null ? upgrade.GetLevel(currentLevel).splashRadius : data.splashRadius;
            int curSplashDamage = upgrade != null ? upgrade.GetLevel(currentLevel).splashDamage : data.splashDamage;


            if (data.projectilePrefab == null || data.fireOrigin == null)
            {
                if (curHasSplash)
                {
                    SplashDamage.ApplySplash(
                        target.transform.position,
                        curSplashRadius,
                        curSplashDamage,
                        curAppliesBurn,
                        curBurnDPS,
                        curBurnDur,
                        curAppliesChill,
                        curChillAmount,
                        curChillDur
                    );
                }
                else
                {
                    target.TakeDamage(rolledDamage);
                    if (curAppliesBurn) target.ApplyBurn(curBurnDPS, curBurnDur);
                    if (curAppliesChill) target.ApplyChill(curChillAmount, curChillDur);
                }

                return;
            }


            GameObject go = Instantiate(data.projectilePrefab, data.fireOrigin.position, Quaternion.identity);
            var proj = go.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Initialize(
                    target,
                    rolledDamage,
                    curAppliesBurn, curBurnDPS, curBurnDur,
                    curAppliesChill, curChillAmount, curChillDur,
                    curHasSplash, curSplashRadius, curSplashDamage,
                    false
                );
            }
            else
            {

                if (curHasSplash)
                {
                    SplashDamage.ApplySplash(
                        target.transform.position,
                        curSplashRadius,
                        curSplashDamage,
                        curAppliesBurn,
                        curBurnDPS,
                        curBurnDur,
                        curAppliesChill,
                        curChillAmount,
                        curChillDur
                    );
                }
                else
                {
                    target.TakeDamage(rolledDamage);
                    if (curAppliesBurn) target.ApplyBurn(curBurnDPS, curBurnDur);
                    if (curAppliesChill) target.ApplyChill(curChillAmount, curChillDur);
                }
                Destroy(go);
            }
        }

        void ApplyDamageToTarget(TD.Enemy target, int damage)
        {
            if (target == null) return;

            if (data.hasSplash)
            {
                SplashDamage.ApplySplash(
                    target.transform.position,
                    data.splashRadius,
                    data.splashDamage,
                    data.appliesBurn,
                    data.burnDPS,
                    data.burnDuration,
                    data.appliesChill,
                    data.chillAmount,
                    data.chillDuration
                );
            }
            else
            {
                target.TakeDamage(damage);
                if (data.appliesBurn) target.ApplyBurn(data.burnDPS, data.burnDuration);
                if (data.appliesChill) target.ApplyChill(data.chillAmount, data.chillDuration);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            var e = other.GetComponent<TD.Enemy>();
            if (e != null && !nearbyEnemies.Contains(e)) nearbyEnemies.Add(e);
        }

        void OnTriggerExit(Collider other)
        {
            var e = other.GetComponent<TD.Enemy>();
            if (e != null) nearbyEnemies.Remove(e);
        }

        public void OnPlaced() { }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, GetCurrentRange());
        }
    }
}
