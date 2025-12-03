using UnityEngine;
using TMPro;

namespace TD.UI
{
    public class UIManagerTMP : MonoBehaviour
    {
        public static UIManagerTMP Instance { get; private set; }

        [Header("HUD (TMP)")]
        public TMP_Text goldText;
        public TMP_Text waveText;
        public TMP_Text livesText;

        [Header("Optional")]
        public GameObject gameOverPanel;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            if (TD.GameManager.Instance != null)
            {
                UpdateGold(TD.GameManager.Instance.gold);
                UpdateWave(TD.GameManager.Instance.currentWave);
                UpdateLives(TD.GameManager.Instance.coreLives);
            }

            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
        }

        public void UpdateGold(int amount)
        {
            if (goldText == null) return;
            goldText.text = $"Gold: {amount}";
        }

        public void UpdateWave(int wave)
        {
            if (waveText == null) return;
            waveText.text = $"Wave: {wave}";
        }

        public void UpdateLives(int lives)
        {
            if (livesText == null) return;
            livesText.text = $"Lives: {lives}";
        }

        public void ShowGameOver()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
        }
    }
}
