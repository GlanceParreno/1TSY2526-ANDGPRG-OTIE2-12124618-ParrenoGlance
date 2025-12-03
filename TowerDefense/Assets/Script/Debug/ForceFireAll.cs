using UnityEngine;
using TD.Towers;

public class ForceFireAll : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            var shooters = FindObjectsOfType<TowerShooter>();
            Debug.Log($"[ForceFireAll] Forcing {shooters.Length} towers to fire.");
            foreach (var s in shooters)
            {
                //s.ForceFireOnce();
            }
        }
    }
}
