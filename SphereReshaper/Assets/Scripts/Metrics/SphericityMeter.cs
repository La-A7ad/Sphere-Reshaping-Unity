using UnityEngine;

namespace SphereReshaper.Metrics
{
    [RequireComponent(typeof(MeshFilter))]
    public class SphericityMeter : MonoBehaviour
    {
        [SerializeField] float smooth = 0.2f;
        public float Score { get; private set; }         // 0..1
        public float MeanRadiusWorld { get; private set; } // for diameter

        Mesh _mesh; MeshFilter _mf;

        void Awake() { _mf = GetComponent<MeshFilter>(); _mesh = _mf.mesh; }

        void LateUpdate() {
            if (_mesh == null) return;
            var v = _mesh.vertices; int n = v.Length; if (n == 0) { Score = 0f; return; }

            // centroid in world
            Vector3 c = Vector3.zero;
            for (int i = 0; i < n; i++) c += transform.TransformPoint(v[i]);
            c /= n;

            // mean/std of world radii
            float mean = 0f;
            for (int i = 0; i < n; i++) mean += (transform.TransformPoint(v[i]) - c).magnitude;
            mean /= n;

            float var = 0f;
            for (int i = 0; i < n; i++) {
                float d = (transform.TransformPoint(v[i]) - c).magnitude;
                float dd = d - mean; var += dd * dd;
            }
            float std = Mathf.Sqrt(var / n);

            float norm = Mathf.Clamp01(1f - (std / Mathf.Max(1e-5f, mean)));
            float k = 1f - Mathf.Exp(-smooth * Time.deltaTime);
            Score = Mathf.Lerp(Score, norm, k);
            MeanRadiusWorld = mean;
        }
    }
}
