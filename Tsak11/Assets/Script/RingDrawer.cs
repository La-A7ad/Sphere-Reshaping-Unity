using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RingDrawer : MonoBehaviour
{
    public enum RingMode { HeightMarker, RadiusTarget }

    [Header("Reference")]
    [SerializeField] public Transform targetBody;

    [Header("General")]
    [SerializeField] public float margin = 0.01f;
    [SerializeField] public float requiredHoldTime = 0.1f;

    [Header("Colors")]
    [SerializeField] public Color correctColor = Color.blue;
    [SerializeField] public Color defaultColor = Color.red;

    [Header("Line")]
    [SerializeField] public int segments = 100;
    [SerializeField] public float lineWidth = 0.02f;

    [Header("Mode")]
    [SerializeField] public RingMode mode = RingMode.HeightMarker;

    [Header("Phase 1 Settings")]
    [SerializeField] public float targetHeight = 0.55f;
    [SerializeField] public float ringRadius = 0.5f;

    [Header("Phase 2 Settings")]
    [SerializeField] private float targetRadius = 0.5f;

    public bool IsScaledCorrectly { get; private set; }
    public float TargetRadius => targetRadius;

    private LineRenderer lineRenderer;
    private float holdTimer = 0f;
    const string MaterialName = "Sprites/Default";

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = true;
        lineRenderer.widthMultiplier = lineWidth;
        var shader = Shader.Find(MaterialName);
        if (shader != null) lineRenderer.material = new Material(shader);
        SetRingColor(defaultColor);
    }

    private void Update()
    {
        if (!targetBody) return;

        bool currentlyCorrect = false;

        if (mode == RingMode.HeightMarker)
        {
            float planetTopY = targetBody.position.y + (targetBody.localScale.y * 0.5f);
            float targetRingTopY = targetBody.position.y + targetHeight;
            currentlyCorrect = Mathf.Abs(planetTopY - targetRingTopY) <= margin;
        }
        else
        {
            float currentRadius = Mathf.Max(targetBody.localScale.x, targetBody.localScale.y, targetBody.localScale.z) * 0.5f;
            currentlyCorrect = Mathf.Abs(currentRadius - targetRadius) <= margin;
        }

        if (currentlyCorrect)
        {
            holdTimer += Time.deltaTime;
            if (!IsScaledCorrectly && holdTimer >= requiredHoldTime) IsScaledCorrectly = true;
            SetRingColor(correctColor);
        }
        else
        {
            holdTimer = 0f;
            IsScaledCorrectly = false;
            SetRingColor(defaultColor);
        }

        if (mode == RingMode.HeightMarker) DrawHeightMarker();
        else DrawRadiusTarget();
    }

    public void SetMode(RingMode newMode) { mode = newMode; ResetHold(); }
    public void SetStaticTarget(float newTargetRadius) { targetRadius = Mathf.Max(0.0001f, newTargetRadius); mode = RingMode.RadiusTarget; ResetHold(); }
    public void ResetHold() { holdTimer = 0f; IsScaledCorrectly = false; }
    public void SetRingRadius(float newRadius) { if (mode == RingMode.HeightMarker) { ringRadius = Mathf.Max(0.0001f, newRadius); targetHeight = Mathf.Max(0.0001f, newRadius); } else { SetStaticTarget(newRadius); } ResetHold(); }

    private void DrawHeightMarker()
    {
        int n = Mathf.Max(3, segments);
        Vector3[] pts = new Vector3[n];
        float step = 360f / n;
        float centerY = targetBody.position.y + (targetHeight - ringRadius);
        Vector3 center = new Vector3(targetBody.position.x, centerY, targetBody.position.z);

        for (int i = 0; i < n; i++)
        {
            float ang = Mathf.Deg2Rad * (i * step);
            float y = Mathf.Sin(ang) * ringRadius;
            float z = Mathf.Cos(ang) * ringRadius;
            pts[i] = center + new Vector3(0f, y, z);
        }
        lineRenderer.positionCount = n;
        lineRenderer.SetPositions(pts);
    }

    private void DrawRadiusTarget()
    {
        int n = Mathf.Max(3, segments);
        Vector3[] pts = new Vector3[n];
        float step = 360f / n;
        Vector3 center = targetBody.position;

        for (int i = 0; i < n; i++)
        {
            float ang = Mathf.Deg2Rad * (i * step);
            float y = Mathf.Sin(ang) * targetRadius;
            float z = Mathf.Cos(ang) * targetRadius;
            pts[i] = center + new Vector3(0f, y, z);
        }
        lineRenderer.positionCount = n;
        lineRenderer.SetPositions(pts);
    }

    private void SetRingColor(Color c) { lineRenderer.startColor = c; lineRenderer.endColor = c; }
}