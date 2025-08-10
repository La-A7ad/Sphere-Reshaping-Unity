using UnityEngine;

namespace SphereReshaper.Deformer
{
    [RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshCollider))]
    public class MouseDeformer : MonoBehaviour
    {
        [Header("Deform")]
        [SerializeField] float deformRadius = 0.25f;
        [SerializeField] float strength = 0.5f;
        [SerializeField, Range(1f,5f)] float falloffPower = 3f;
        [SerializeField] Camera cam;

        Mesh _mesh; MeshCollider _col; Vector3[] _verts;
        bool _dragging; Vector3 _lastHit;

        void Awake() {
            var mf = GetComponent<MeshFilter>();
            _mesh = mf.mesh;           // instance
            _verts = _mesh.vertices;
            _col = GetComponent<MeshCollider>();
            if (!cam) cam = Camera.main;
            _mesh.MarkDynamic();
        }

        public bool allowDeform = true;

        void Update() {
            if (!allowDeform) return;
            if (!enabled || cam == null) return;

            if (Input.GetMouseButton(0)) {
                var ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, 1000f) && hit.collider.transform == transform) {
                    DeformLocalAt(hit);
                    _dragging = true; _lastHit = hit.point;
                }
            } else if (_dragging) {
                if (_col) { _col.sharedMesh = null; _col.sharedMesh = _mesh; } // refresh once
                _dragging = false;
            }
        }

        void DeformLocalAt(RaycastHit hit) {
            var lh = transform.InverseTransformPoint(hit.point);
            var ln = transform.InverseTransformDirection(hit.normal).normalized;

            for (int i = 0; i < _verts.Length; i++) {
                float d = Vector3.Distance(_verts[i], lh);
                if (d > deformRadius) continue;
                float w = Mathf.Pow(1f - (d / deformRadius), falloffPower);
                _verts[i] += ln * (strength * w * Time.deltaTime);
            }
            _mesh.vertices = _verts;
            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();
        }

        void OnDrawGizmosSelected() {
            if (_dragging) { Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(_lastHit, deformRadius); }
        }
    }
}
