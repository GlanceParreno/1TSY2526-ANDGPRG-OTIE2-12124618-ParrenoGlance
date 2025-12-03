using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TD.Towers
{
    [RequireComponent(typeof(TowerData))]
    public class TowerTargeting : MonoBehaviour
    {
        TowerData data;
        SphereCollider rangeTrigger;
        readonly List<TD.Enemy> tracked = new List<TD.Enemy>();

        public enum TargetPriority { Closest, LowestHealth, HighestHealth, ClosestToCore }
        public TargetPriority priority = TargetPriority.Closest;

        [Header("Debug")]
        public bool debugLogs = false;
        public bool drawGizmos = true;

        TD.Enemy lastGizmoTarget;

        void Awake()
        {
            data = GetComponent<TowerData>();

            rangeTrigger = GetComponent<SphereCollider>();
            if (rangeTrigger == null)
            {
                rangeTrigger = gameObject.AddComponent<SphereCollider>();
                rangeTrigger.isTrigger = true;
                rangeTrigger.radius = data.range;

                if (debugLogs)
                    Debug.Log($"[Targeting] Added SphereCollider to {name} with radius {rangeTrigger.radius}");
            }
            else
            {
                rangeTrigger.isTrigger = true;
                rangeTrigger.radius = data.range;
            }
        }

        void Update()
        {
            if (data != null && rangeTrigger != null &&
                Mathf.Abs(rangeTrigger.radius - data.range) > 0.01f)
                rangeTrigger.radius = data.range;

            tracked.RemoveAll(e => e == null || e.CurrentHealth() <= 0);
        }

        void OnTriggerEnter(Collider other)
        {
            var e = other.GetComponent<TD.Enemy>();
            if (e != null)
            {
                if (e.isFlying && !data.canTargetFlying) return;
                if (!e.isFlying && !data.canTargetGround) return;

                if (!tracked.Contains(e))
                {
                    tracked.Add(e);

                    if (debugLogs)
                        Debug.Log($"[Targeting] {name} ENEMY ENTER: {e.name} (Tracked={tracked.Count})");
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            var e = other.GetComponent<TD.Enemy>();
            if (e != null && tracked.Contains(e))
            {
                tracked.Remove(e);

                if (debugLogs)
                    Debug.Log($"[Targeting] {name} ENEMY EXIT: {e.name} (Tracked={tracked.Count})");
            }
        }

        public bool HasTargets() => tracked.Count > 0;

        public TD.Enemy GetBestTarget()
        {
            tracked.RemoveAll(e => e == null || e.CurrentHealth() <= 0);

            if (tracked.Count == 0)
            {
                lastGizmoTarget = null;
                return null;
            }

            TD.Enemy t = null;

            switch (priority)
            {
                case TargetPriority.LowestHealth:
                    t = tracked.OrderBy(e => e.CurrentHealth()).First();
                    break;

                case TargetPriority.HighestHealth:
                    t = tracked.OrderByDescending(e => e.CurrentHealth()).First();
                    break;

                case TargetPriority.ClosestToCore:
                    var core = GameObject.FindGameObjectWithTag("Core");
                    if (core == null)
                        t = tracked.First();
                    else
                        t = tracked.OrderBy(e => Vector3.Distance(e.transform.position, core.transform.position)).First();
                    break;

                case TargetPriority.Closest:
                default:
                    t = tracked.OrderBy(e => Vector3.Distance(transform.position, e.transform.position)).First();
                    break;
            }

            if (debugLogs && t != null)
                Debug.Log($"[Targeting] {name} BEST TARGET: {t.name}");

            lastGizmoTarget = t;
            return t;
        }

        public int GetTrackedCount() => tracked.Count;
        void OnDrawGizmos()
        {
            if (!drawGizmos) return;
            if (data == null) return;


            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, data.range);


            if (lastGizmoTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, lastGizmoTarget.transform.position);
            }
        }
    }
}
