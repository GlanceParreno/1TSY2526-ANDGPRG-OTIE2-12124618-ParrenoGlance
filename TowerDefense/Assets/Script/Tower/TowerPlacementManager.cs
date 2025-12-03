using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using TD.UI;

namespace TD.Towers
{
    public class TowerPlacementManager : MonoBehaviour
    {
        public static TowerPlacementManager Instance { get; private set; }

        [Header("Placement")]
        public LayerMask placementLayerMask = ~0;
        public float placementHeightOffset = 0.02f;
        public float minNavmeshDistance = 0.1f;

        [Header("Overlap Check")]
        public float overlapRadius = 1.0f;
        public LayerMask towerLayerMask;

        [Header("Ghost visuals")]
        public Material validMaterial;
        public Material invalidMaterial;
        [Tooltip("Default ghost transparency multiplier (applied to original material color)")]
        public float ghostAlpha = 0.6f;

        GameObject currentGhost;
        GameObject currentPrefab;
        int currentCost;
        bool isPlacing = false;

        Camera cam;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            cam = Camera.main;
            if (towerLayerMask.value == 0) towerLayerMask = 1 << 0;
        }

        void Update()
        {
            if (!isPlacing) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            UpdateGhostPosition();

            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement();
            }

            if (Input.GetMouseButtonDown(0))
            {
                TryPlace();
            }
        }


        public void StartPlacementWithPrefab(GameObject prefab)
        {
            if (prefab == null) return;
            var td = prefab.GetComponent<TowerData>();
            int cost = td != null ? td.cost : 0;
            StartPlacementWithPrefab(prefab, cost);
        }


        public void StartPlacementWithPrefab(GameObject prefab, int cost)
        {
            if (prefab == null) return;
            if (isPlacing) CancelPlacement();

            currentPrefab = prefab;
            currentCost = cost;
            CreateGhost(prefab);
            isPlacing = true;
        }

        void CancelPlacement()
        {
            isPlacing = false;
            DestroyGhost();
            currentPrefab = null;
            currentCost = 0;
        }

        void CreateGhost(GameObject prefab)
        {
            DestroyGhost();
            currentGhost = Instantiate(prefab);
            MonoBehaviour[] comps = currentGhost.GetComponentsInChildren<MonoBehaviour>();
            for (int i = 0; i < comps.Length; i++) { Destroy(comps[i]); }

            var rends = currentGhost.GetComponentsInChildren<Renderer>();
            foreach (var r in rends)
            {
                Material mat = new Material(r.sharedMaterial);
                Color c = mat.color; c.a = ghostAlpha;
                mat.color = c;
                r.material = mat;
            }
        }

        void DestroyGhost()
        {
            if (currentGhost != null) Destroy(currentGhost);
            currentGhost = null;
        }

        void UpdateGhostPosition()
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 999f, placementLayerMask))
            {
                Vector3 pos = hit.point;
                pos.y += placementHeightOffset;

                bool onNavMesh = false;
                NavMeshHit nmHit;
                if (NavMesh.SamplePosition(pos, out nmHit, minNavmeshDistance, NavMesh.AllAreas))
                    onNavMesh = true;

                bool overlap = Physics.CheckSphere(pos, overlapRadius, towerLayerMask, QueryTriggerInteraction.Ignore);

                bool valid = !onNavMesh && !overlap;

                if (currentGhost != null)
                {
                    currentGhost.transform.position = pos;
                    Vector3 forward = cam.transform.forward; forward.y = 0; if (forward.sqrMagnitude > 0.001f) currentGhost.transform.rotation = Quaternion.LookRotation(forward);
                    SetGhostMaterial(valid ? validMaterial : invalidMaterial);
                }
            }
        }

        void SetGhostMaterial(Material mat)
        {
            if (currentGhost == null || mat == null) return;
            var rends = currentGhost.GetComponentsInChildren<Renderer>();
            foreach (var r in rends)
            {
                Color prev = r.material.color;
                Material copy = new Material(mat);
                copy.color = new Color(copy.color.r, copy.color.g, copy.color.b, prev.a);
                r.material = copy;
            }
        }

        void TryPlace()
        {
            if (currentGhost == null || currentPrefab == null) return;

            Vector3 pos = currentGhost.transform.position;

            NavMeshHit nmHit;
            bool onNavMesh = NavMesh.SamplePosition(pos, out nmHit, minNavmeshDistance, NavMesh.AllAreas);
            bool overlap = Physics.CheckSphere(pos, overlapRadius, towerLayerMask, QueryTriggerInteraction.Ignore);
            bool valid = !onNavMesh && !overlap;

            if (!valid) return;


            if (!TD.GameManager.Instance.SpendGold(currentCost))
            {

                return;
            }

            GameObject placed = Instantiate(currentPrefab, pos, currentGhost.transform.rotation);

            int layer = (int)Mathf.Log(towerLayerMask.value, 2);
            SetLayerRecursive(placed, layer);

            var towerComp = placed.GetComponentInChildren<TowerController>();
            if (towerComp != null) towerComp.OnPlaced();

            CancelPlacement();
        }

        void SetLayerRecursive(GameObject go, int layer)
        {
            if (go == null) return;
            go.layer = layer;
            foreach (Transform t in go.transform) SetLayerRecursive(t.gameObject, layer);
        }


    }
}
