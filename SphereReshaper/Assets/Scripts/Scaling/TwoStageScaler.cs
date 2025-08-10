using UnityEngine;
using SphereReshaper.Metrics;

namespace SphereReshaper.Scaling
{
    /// <summary>
    /// Stage 1: LMB drag adjusts Y only until within tolerance -> locks to targetYScale.
    /// Stage 2: LMB drag scales uniformly (X=Y=Z) within min/max clamps.
    /// </summary>
    public class TwoStageScaler : MonoBehaviour
    {
        [Header("Stage 1: Y-only")]
        public float targetYScale = 1.0f;
        [Range(0f, 0.2f)] public float yTolerancePct = 0.02f; // ±2%
        public float yDragSpeed = 2.0f;

        [Header("Stage 2: Uniform (mouse drag)")]
        public float uniformSensitivity = 1.5f;
        public float minScale = 0.2f;
        public float maxScale = 5f;

        [Header("Target Diameter (world)")]
        public SphericityMeter meter;        // assign for accurate world diameter
        public float targetDiameter = 1.2f;  // world units
        public float diameterTolerancePct = 0.05f; // ±5%

        public bool YLocked { get; private set; } = false;

        float _lastMouseY;
        bool _dragging;

        void OnEnable() { _dragging = false; }

        void Update() {
            if (Input.GetMouseButtonDown(0)) { _dragging = true; _lastMouseY = Input.mousePosition.y; }
            if (Input.GetMouseButtonUp(0))   { _dragging = false; }

            if (!_dragging) return;

            float yNow = Input.mousePosition.y;
            float dy = (yNow - _lastMouseY) / Mathf.Max(1f, Screen.height); // normalized per screen
            _lastMouseY = yNow;

            if (!YLocked) {
                // ---- Stage 1: adjust Y only ----
                float newY = transform.localScale.y * (1f + dy * yDragSpeed);
                newY = Mathf.Clamp(newY, minScale, maxScale);
                var s = transform.localScale; s.y = newY; transform.localScale = s;

                // IF: Y is now correct -> snap & lock
                if (IsYWithinTolerance()) {
                    s = transform.localScale; s.y = targetYScale; transform.localScale = s;
                    YLocked = true;
                }
            } else {
                // ---- Stage 2: uniform scaling ----
                float factor = 1f + dy * uniformSensitivity;
                factor = Mathf.Clamp(factor, 0.5f, 1.5f); // per-frame clamp
                var s = transform.localScale * factor;
                float uni = Mathf.Clamp(s.x, minScale, maxScale);
                transform.localScale = new Vector3(uni, uni, uni);
            }
        }

        // ===== IF conditions you asked for =====
        public bool IsYWithinTolerance() {
            float y = transform.localScale.y;
            return Mathf.Abs(y - targetYScale) <= targetYScale * yTolerancePct;
        }

        public bool IsDiameterOk() {
            float d = GetDiameter();
            float errPct = Mathf.Abs(d - targetDiameter) / Mathf.Max(0.0001f, targetDiameter);
            return errPct <= diameterTolerancePct;
        }

        public float GetDiameter() {
            if (meter && meter.MeanRadiusWorld > 0f) return meter.MeanRadiusWorld * 2f;
            var r = GetComponent<Renderer>();
            return r ? r.bounds.size.x : transform.lossyScale.x; // fallback
        }
    }
}
