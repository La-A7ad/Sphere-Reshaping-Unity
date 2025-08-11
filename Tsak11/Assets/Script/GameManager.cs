using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum Phase { PullToSphere, UniformResize }

    [Header("Phase")]
    public Phase phase = Phase.PullToSphere;

    [Header("Controllers")]
    [SerializeField] private YScaler yScalerScript;   // assign in Inspector
    [SerializeField] private Scaler  scalerScript;    // assign in Inspector

    [Header("Rings")]
    [SerializeField] private RingDrawer earthRing;
    [SerializeField] private RingDrawer moonRing;
    [SerializeField] private RingDrawer sunRing;

    [Header("Planets")]
    [SerializeField] private Transform earth;         // Earth root transform (baseline)

    [Header("Phase 2 Target Ratios (relative to Earth)")]
    [SerializeField] private float moonToEarthRatio = 0.27f;
    [SerializeField] private float sunToEarthRatio  = 109f;

    [Header("Audio (optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip phase2Clip;
    [SerializeField] private AudioClip finishClip;

    private bool phase2Announced = false;
    private bool finishedAnnounced = false;

    // -------------------- Lifecycle --------------------

    private void Start()
    {
        SetPhase(Phase.PullToSphere);
        AutoCalibratePhase1Targets(); // make Phase 1 targets reachable based on current sizes
    }

    private void Update()
    {
        if (phase == Phase.PullToSphere && AllPlanetsScaledCorrectly())
        {
            SetPhase(Phase.UniformResize);
            SetupPhase2Targets();

            if (!phase2Announced && audioSource && phase2Clip)
            {
                audioSource.PlayOneShot(phase2Clip);
                phase2Announced = true;
            }
        }
        else if (phase == Phase.UniformResize && AllPlanetsScaledCorrectly())
        {
            if (!finishedAnnounced && audioSource && finishClip)
            {
                audioSource.PlayOneShot(finishClip);
                finishedAnnounced = true;
            }
            // TODO: advance game flow, show UI, load next scene, etc.
        }
    }

    // -------------------- Phase control --------------------

    public void SetPhase(Phase p)
    {
        phase = p;
        if (yScalerScript) yScalerScript.enabled = (p == Phase.PullToSphere);
        if (scalerScript)  scalerScript.enabled  = (p == Phase.UniformResize);
        Debug.Log($"[GameManager] Phase -> {phase}. YScaler={yScalerScript?.enabled} Scaler={scalerScript?.enabled}");
    }

    public bool AllPlanetsScaledCorrectly()
    {
        return earthRing && moonRing && sunRing &&
               earthRing.IsScaledCorrectly &&
               moonRing.IsScaledCorrectly &&
               sunRing.IsScaledCorrectly;
    }

    // -------------------- Phase 1 helpers --------------------

    private void AutoCalibratePhase1Targets()
    {
        Calibrate(earthRing);
        Calibrate(moonRing);
        Calibrate(sunRing);
    }

    private void Calibrate(RingDrawer ring)
    {
        if (!ring || !ring.targetBody) return;
        float r = GetWorldRadius(ring.targetBody);
        // Phase 1 uses HeightMarker: top == center + targetHeight; target is the radius in world units
        ring.mode = RingDrawer.RingMode.HeightMarker;
        ring.targetHeight = r;
        ring.ringRadius   = Mathf.Max(0.01f, r * 0.9f); // purely visual in Phase 1
        ring.margin       = Mathf.Max(0.02f, ring.margin); // keep at least a reasonable tolerance
        ring.ResetHold();
    }

    private float GetWorldRadius(Transform t)
    {
        // world radius based on lossy scale (handles scaled parents correctly)
        Vector3 s = t.lossyScale;
        return Mathf.Max(s.x, s.y, s.z) * 0.5f;
    }

    // -------------------- Phase 2 setup --------------------

    private float GetEarthRadius()
    {
        if (!earth) return 0.5f;
        Vector3 s = earth.lossyScale; // world scale in case Earth has a scaled parent
        return Mathf.Max(s.x, s.y, s.z) * 0.5f;
    }

    public void SetupPhase2Targets()
    {
        float earthR = GetEarthRadius();
        if (earthRing) earthRing.SetStaticTarget(earthR);
        if (moonRing)  moonRing.SetStaticTarget(earthR * Mathf.Max(0.0001f, moonToEarthRatio));
        if (sunRing)   sunRing.SetStaticTarget(earthR * Mathf.Max(0.0001f, sunToEarthRatio));
    }
}
