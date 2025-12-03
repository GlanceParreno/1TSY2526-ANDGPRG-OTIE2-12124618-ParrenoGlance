using UnityEngine;
using UnityEngine.EventSystems;

namespace TD.UI
{
    public class TowerSelectionManager : MonoBehaviour
    {
        public UpgradeUI upgradeUI;

        Camera cam;

        void Awake()
        {
            cam = Camera.main;
        }

        void Update()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, 100f))
                {
                    var t = hit.collider.GetComponentInParent<TD.Towers.TowerController>();
                    if (t != null)
                    {

                        if (upgradeUI != null) upgradeUI.ShowFor(t);
                        return;
                    }
                }

                if (upgradeUI != null) upgradeUI.Hide();
            }
        }
    }
}
