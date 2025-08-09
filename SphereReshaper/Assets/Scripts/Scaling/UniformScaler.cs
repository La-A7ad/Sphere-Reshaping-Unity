using UnityEngine;
namespace SphereReshaper.Scaling
{
    public class UniformScaler : MonoBehaviour
    {
        [SerializeField] private float scaleStep = 0.05f;
        private void Update(){
            float s = Input.mouseScrollDelta.y;
            if (Mathf.Abs(s)>0.0001f){ transform.localScale *= 1f + (s*scaleStep); }
        }
        public float GetDiameter(){ return transform.lossyScale.x; }
    }
}
