using UnityEngine;

namespace TD.Towers
{
    [DisallowMultipleComponent]
    public class TowerData : MonoBehaviour
    {
        [Header("Identity")]
        public string towerName = "Tower";

        [Header("Economy")]
        public int cost = 100;
        public float buildTime = 10f;

        [Header("Combat")]
        public int minDamage = 5;
        public int maxDamage = 10;
        public float range = 7f;
        public float fireRate = 1f;

        [Header("Splash")]
        public bool hasSplash = false;
        public float splashRadius = 1.5f;
        public int splashDamage = 10;

        [Header("Targeting")]
        public bool canTargetFlying = true;
        public bool canTargetGround = true;

        [Header("Special")]
        public bool appliesChill = false;
        public float chillAmount = 0.3f;
        public float chillDuration = 3f;

        public bool appliesBurn = false;
        public int burnDPS = 5;
        public float burnDuration = 3f;

        [Header("Visuals")]
        public GameObject projectilePrefab;
        public Transform fireOrigin;

        public int RollDamage()
        {
            int min = Mathf.Max(0, Mathf.Min(minDamage, maxDamage));
            int max = Mathf.Max(minDamage, maxDamage);
            return Random.Range(min, max + 1);
        }

        void OnValidate()
        {
            if (minDamage < 0) minDamage = 0;
            if (maxDamage < 0) maxDamage = 0;
            if (range < 0.1f) range = 0.1f;
            if (fireRate <= 0f) fireRate = 1f;
            if (cost < 0) cost = 0;
        }
    }
}
