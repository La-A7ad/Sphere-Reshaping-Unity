using UnityEngine;
namespace SphereReshaper.Metrics
{
    [RequireComponent(typeof(MeshFilter))]
    public class SphericityMeter : MonoBehaviour
    {
        [SerializeField] private float smooth = 0.2f;
        public float Score { get; private set; }
        private Mesh _mesh;
        private void Awake(){ _mesh = GetComponent<MeshFilter>().mesh; }
        private void LateUpdate(){
            if (_mesh == null) return;
            var v = _mesh.vertices; int n = v.Length; if (n==0){ Score=0f; return; }
            float sum=0f; for(int i=0;i<n;i++) sum += v[i].magnitude;
            float mean = sum/n;
            float var=0f; for(int i=0;i<n;i++){ float d=v[i].magnitude-mean; var+=d*d; }
            float std = Mathf.Sqrt(var/n);
            float norm = Mathf.Clamp01(1f - (std/Mathf.Max(0.0001f, mean)));
            Score = Mathf.Lerp(Score, norm, 1f - Mathf.Exp(-smooth * Time.deltaTime));
        }
    }
}
