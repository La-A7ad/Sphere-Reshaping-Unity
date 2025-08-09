using UnityEngine; using SphereReshaper.Metrics; using SphereReshaper.Scaling;
namespace SphereReshaper.Game
{
    public enum Phase { Deform, Resize, Done }
    public class GameFlow : MonoBehaviour
    {
        [SerializeField] private SphericityMeter meter;
        [SerializeField] private UniformScaler scaler;
        [SerializeField] private float sphericityThreshold = 0.95f;
        [SerializeField] private float targetDiameter = 1.0f;
        [SerializeField] private float targetTolerancePct = 0.05f;
        public Phase Current { get; private set; } = Phase.Deform;
        private void Update(){
            switch(Current){
                case Phase.Deform:
                    if (meter && meter.Score >= sphericityThreshold) Current = Phase.Resize; break;
                case Phase.Resize:
                    if (scaler != null){
                        float d = scaler.GetDiameter();
                        float pct = Mathf.Abs(d - targetDiameter) / Mathf.Max(0.0001f, targetDiameter);
                        if (pct <= targetTolerancePct){ Current = Phase.Done; OnCompleted(); }
                    } break;
            }
        }
        private void OnCompleted(){ Debug.Log("All targets met! Trigger narration here."); }
    }
}
