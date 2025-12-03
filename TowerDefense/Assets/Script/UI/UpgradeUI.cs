using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TD.Towers;

namespace TD.UI
{
    public class UpgradeUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public GameObject panel;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI currentStatsText;
        public TextMeshProUGUI nextStatsText;
        public TextMeshProUGUI costText;
        public Button upgradeButton;

        [Header("Buttons")]
        public Button closeButton;

        TowerController selectedTower;

        void Awake()
        {
            if (panel != null)
                panel.SetActive(false);

            if (upgradeButton != null)
                upgradeButton.onClick.AddListener(OnUpgradePressed);

            if (closeButton != null)
                closeButton.onClick.AddListener(OnClosePressed);
        }
        public void ShowFor(TowerController tower)
        {
            selectedTower = tower;
            UpdateUI();
            if (panel != null)
                panel.SetActive(true);
        }

        public void Hide()
        {
            selectedTower = null;
            if (panel != null)
                panel.SetActive(false);
        }
        void UpdateUI()
        {
            if (selectedTower == null)
            {
                Hide();
                return;
            }

            var td = selectedTower.GetComponent<TowerData>();
            titleText.text = td != null ? td.towerName : selectedTower.name;
            levelText.text = $"Level: {selectedTower.currentLevel}";

            currentStatsText.text = GetStatsString(selectedTower, selectedTower.currentLevel);

            int nextCost = selectedTower.GetNextUpgradeCost();
            if (nextCost < 0)
            {
                nextStatsText.text = "Max Level";
                costText.text = "-";
                upgradeButton.interactable = false;
            }
            else
            {
                nextStatsText.text = GetStatsString(selectedTower, selectedTower.currentLevel + 1);
                costText.text = $"{nextCost}g";
                upgradeButton.interactable = TD.GameManager.Instance != null &&
                                             TD.GameManager.Instance.gold >= nextCost;
            }
        }

        string GetStatsString(TowerController t, int level)
        {
            var up = t.GetComponent<TowerUpgrade>();
            if (up != null)
            {
                var lvl = up.GetLevel(level);
                return $"DMG: {lvl.minDamage}-{lvl.maxDamage}\n" +
                       $"Rate: {lvl.fireRate:F2}s\n" +
                       $"Range: {lvl.range:F1}\n" +
                       $"Burn: {(lvl.appliesBurn ? $"{lvl.burnDPS} DPS x{lvl.burnDuration}s" : "-")}\n" +
                       $"Chill: {(lvl.appliesChill ? $"{lvl.chillAmount * 100f:F0}% x{lvl.chillDuration}s" : "-")}\n" +
                       $"Splash: {(lvl.hasSplash ? $"{lvl.splashDamage} ({lvl.splashRadius}m)" : "-")}";
            }
            else
            {
                var d = t.GetComponent<TowerData>();
                return $"DMG: {d.minDamage}-{d.maxDamage}\n" +
                       $"Rate: {d.fireRate:F2}s\n" +
                       $"Range: {d.range:F1}\n" +
                       $"Burn: {(d.appliesBurn ? $"{d.burnDPS} DPS x{d.burnDuration}s" : "-")}\n" +
                       $"Chill: {(d.appliesChill ? $"{d.chillAmount * 100f:F0}% x{d.chillDuration}s" : "-")}\n" +
                       $"Splash: {(d.hasSplash ? $"{d.splashDamage} ({d.splashRadius}m)" : "-")}";
            }
        }
        void OnUpgradePressed()
        {
            if (selectedTower == null) return;

            bool ok = selectedTower.TryUpgrade();
            if (ok)
                UpdateUI();
        }

        public void OnClosePressed()
        {
            Hide();
        }
        void Update()
        {
            if (panel != null && panel.activeSelf)
            {

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Hide();
                }

                if (selectedTower != null)
                {
                    int cost = selectedTower.GetNextUpgradeCost();
                    upgradeButton.interactable =
                        (cost >= 0 &&
                        TD.GameManager.Instance != null &&
                        TD.GameManager.Instance.gold >= cost);
                }
            }
        }
    }
}
