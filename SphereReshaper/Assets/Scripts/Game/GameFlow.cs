using UnityEngine;
using UnityEngine.Events;
using SphereReshaper.Metrics;
using SphereReshaper.Scaling;
using SphereReshaper.Deformer;

namespace SphereReshaper.Game
{
    public class GameFlow : MonoBehaviour
    {
        public enum Phase { Deform, Resize, Done }

        [Header("References")]
        [SerializeField] private SphericityMeter meter;
        [SerializeField] private TwoStageScaler scaler;
        [SerializeField] private MouseDeformer deformer;
        [SerializeField] private RingIndicator ring;   // LineRenderer-based circle

        [Header("Tuning")]
        [Range(0.7f, 0.99f)] public float sphericityThreshold = 0.95f;
        [Tooltip("Target final world diameter for the planet.")]
        public float targetDiameter = 1.2f;
        [Range(0.0f, 0.2f)] public float diameterTolerancePct = 0.05f; // ±5%

        [Header("Events")]
        public UnityEvent onEnterDeform;
        public UnityEvent onEnterResize;
        public UnityEvent onDone;

        public Phase Current { get; private set; } = Phase.Deform;

        void Reset() {
            meter    = FindObjectOfType<SphericityMeter>();
            scaler   = FindObjectOfType<TwoStageScaler>();
            deformer = FindObjectOfType<MouseDeformer>();
            ring     = FindObjectOfType<RingIndicator>();
        }

        void Start() {
            if (scaler) {
                scaler.targetDiameter       = targetDiameter;
                scaler.diameterTolerancePct = diameterTolerancePct;
            }
            EnterDeform();
        }

        void Update() {
            switch (Current) {
                case Phase.Deform:
                    if (meter && meter.Score >= sphericityThreshold) EnterResize();
                    break;

                case Phase.Resize:
                    if (!scaler) break;

                    // Show the ring only after Y has locked (stage 2)
                    if (scaler.YLocked) {
                        if (ring && !ring.gameObject.activeSelf) {
                            ring.gameObject.SetActive(true);
                            ring.SetRadius(targetDiameter * 0.5f);
                            ring.Redraw();
                        }

                        // Drive ring feedback by diameter error
                        if (ring) {
                            float err = Mathf.Abs(scaler.GetDiameter() - targetDiameter)
                                        / Mathf.Max(0.0001f, targetDiameter);
                            float ratio = Mathf.Clamp01(err / scaler.diameterTolerancePct); // 1=far, 0=ok
                            ring.SetError(ratio);
                        }

                        if (scaler.IsDiameterOk()) EnterDone();
                    }
                    break;
            }
        }

        void EnterDeform() {
    Current = Phase.Deform;
    if (deformer) { deformer.enabled = true;  ToggleMeshCollider(deformer, true); }
    if (scaler)   scaler.enabled   = false;
    if (ring)     ring.gameObject.SetActive(false);
    onEnterDeform?.Invoke();
}

void EnterResize() {
    Current = Phase.Resize;
    if (deformer) { deformer.allowDeform = false; deformer.enabled = false; ToggleMeshCollider(deformer, false); } // <- stops bulge
    if (scaler)   scaler.enabled   = true;
    if (ring)     ring.gameObject.SetActive(false); // will show after Y locks
    onEnterResize?.Invoke();
}

void EnterDone() {
    Current = Phase.Done;
    if (deformer) { deformer.allowDeform = true; deformer.enabled = false; ToggleMeshCollider(deformer, false); }
    if (scaler)   scaler.enabled   = false;
    if (ring)     ring.gameObject.SetActive(false);
    onDone?.Invoke();
}

static void ToggleMeshCollider(SphereReshaper.Deformer.MouseDeformer d, bool on) {
    var col = d.GetComponent<MeshCollider>();
    if (col) col.enabled = on;   // no collider = no raycast hits = no deformation
}

    }
}
