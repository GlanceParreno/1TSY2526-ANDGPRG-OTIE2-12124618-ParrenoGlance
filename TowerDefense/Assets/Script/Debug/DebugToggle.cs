// using UnityEngine;
// using TD.Towers;

// // Press U to toggle debugLogs, I to toggle drawGizmos for all towers in scene
// public class DebugToggles : MonoBehaviour
// {
//     public bool targetingsDebug = true;
//     public bool shootersDebug = true;
//     public bool gizmosOn = true;

//     void Update()
//     {
//         // Toggle debug logs for all towers
//         if (Input.GetKeyDown(KeyCode.U))
//         {
//             ToggleDebugLogs();
//         }

//         // Toggle gizmo drawing flags on components (so their OnDrawGizmos runs)
//         if (Input.GetKeyDown(KeyCode.I))
//         {
//             ToggleGizmos();
//         }
//     }

//     void ToggleDebugLogs()
//     {
//         var tg = FindObjectsOfType<TowerTargeting>();
//         var sh = FindObjectsOfType<TowerShooter>();
//         bool newVal = !targetingsDebug;
//         targetingsDebug = newVal;
//         foreach (var t in tg) t.debugLogs = newVal;
//         bool newVal2 = !shootersDebug;
//         shootersDebug = newVal2;
//         //foreach (var s in sh) s.debugLogs = newVal2;
//         Debug.Log($"[DebugToggles] Set TowerTargeting.debugLogs={newVal}, TowerShooter.debugLogs={newVal2}");
//     }

//     void ToggleGizmos()
//     {
//         var tg = FindObjectsOfType<TowerTargeting>();
//         var sh = FindObjectsOfType<TowerShooter>();
//         bool newVal = !gizmosOn;
//         gizmosOn = newVal;
//         foreach (var t in tg) t.drawGizmos = newVal;
//         foreach (var s in sh) s.drawGizmos = newVal;
//         Debug.Log($"[DebugToggles] Set drawGizmos={newVal} on {tg.Length + sh.Length} components");
//     }
// }
