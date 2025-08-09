using UnityEngine; using UnityEngine.UI; using SphereReshaper.Metrics;
namespace SphereReshaper.UI
{
    public class ProgressUI : MonoBehaviour
    {
        [SerializeField] private SphericityMeter meter;
        [SerializeField] private Slider slider;
        private void Update(){ if(meter && slider) slider.value = meter.Score; }
    }
}
