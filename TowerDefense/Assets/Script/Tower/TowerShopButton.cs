using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TD.Towers
{
    public class TowerShopButton : MonoBehaviour
    {
        public GameObject towerPrefab;
        public TMP_Text priceText;
        Button btn;

        void Awake()
        {
            btn = GetComponent<Button>();
            if (priceText != null)
            {
                var td = towerPrefab != null ? towerPrefab.GetComponent<TowerData>() : null;
                priceText.text = td != null ? td.cost.ToString() : "0";
            }
            btn.onClick.AddListener(OnClick);
        }

        void OnClick()
        {
            if (towerPrefab == null) return;
            TowerPlacementManager.Instance?.StartPlacementWithPrefab(towerPrefab);
        }
    }
}
