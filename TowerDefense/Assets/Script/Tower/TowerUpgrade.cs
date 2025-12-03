using UnityEngine;

namespace TD.Towers
{
    [DisallowMultipleComponent]
    public class TowerUpgrade : MonoBehaviour
    {
        [System.Serializable]
        public class Level
        {
            public int upgradeCost = 50;


            public int minDamage = 5;
            public int maxDamage = 10;
            public float fireRate = 1f;
            public float range = 7f;


            public bool appliesChill = false;
            public float chillAmount = 0f;
            public float chillDuration = 0f;

            public bool appliesBurn = false;
            public int burnDPS = 0;
            public float burnDuration = 0f;

            public bool hasSplash = false;
            public float splashRadius = 0f;
            public int splashDamage = 0;
        }

        [Tooltip("Define levels. Index 0 is the base level (level 0).")]
        public Level[] levels = new Level[1];

        public int GetMaxLevels()
        {
            return (levels == null) ? 1 : levels.Length;
        }

        public Level GetLevel(int level)
        {
            if (levels == null || levels.Length == 0)
            {
                return new Level();
            }
            level = Mathf.Clamp(level, 0, levels.Length - 1);
            return levels[level];
        }

        public int GetNextLevelCost(int currentLevel)
        {
            if (levels == null) return -1;
            int next = currentLevel + 1;
            if (next < 0 || next >= levels.Length) return -1;
            return levels[next].upgradeCost;
        }
    }
}
