using UnityEngine;
using TMPro;

namespace TD.UI
{
    public class CoreHealthBarTMP : MonoBehaviour
    {
        public TMP_Text livesText;

        Transform cam;

        void Start()
        {
            cam = Camera.main != null ? Camera.main.transform : null;
            UpdateDisplay();
        }

        void Update()
        {
            if (cam != null)
                transform.LookAt(transform.position + cam.forward);

            UpdateDisplay();
        }

        void UpdateDisplay()
        {
            if (livesText == null) return;

            int lives = TD.GameManager.Instance != null ? TD.GameManager.Instance.coreLives : 0;
            livesText.text = $"Core: {lives}";
        }
    }
}
