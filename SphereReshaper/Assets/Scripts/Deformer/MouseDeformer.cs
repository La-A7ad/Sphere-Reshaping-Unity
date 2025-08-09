using UnityEngine;
namespace SphereReshaper.Deformer
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
    public class MouseDeformer : MonoBehaviour
    {
        [SerializeField] private float deformRadius = 0.25f;
        [SerializeField] private float strength = 0.5f;
        [SerializeField] private Camera cam;
        private Mesh _mesh;
        private Vector3[] _verts;
        private void Awake(){ _mesh = GetComponent<MeshFilter>().mesh; _verts = _mesh.vertices; if(!cam) cam = Camera.main; }
        private void Update(){
            if (Input.GetMouseButton(0)){
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, 100f) && hit.transform == transform){
                    Vector3 lh = transform.InverseTransformPoint(hit.point);
                    Vector3 ln = transform.InverseTransformDirection(hit.normal).normalized;
                    for(int i=0;i<_verts.Length;i++){
                        float d = Vector3.Distance(_verts[i], lh);
                        if (d <= deformRadius){
                            float fall = 1f - (d/deformRadius);
                            _verts[i] += ln * (strength * fall * Time.deltaTime);
                        }
                    }
                    _mesh.vertices = _verts; _mesh.RecalculateNormals(); _mesh.RecalculateBounds();
                    var col = GetComponent<MeshCollider>(); if(col){ col.sharedMesh = null; col.sharedMesh = _mesh; }
                }
            }
        }
    }
}
