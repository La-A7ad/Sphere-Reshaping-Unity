using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RingIndicator : MonoBehaviour
{
    public int segments = 128;
    public float radius = 0.5f;
    public Vector3 normal = Vector3.up;

    LineRenderer lr;

    void Awake() {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = segments + 1;
    }

    public void SetRadius(float r) { radius = Mathf.Max(0.001f, r); Redraw(); }

    // t: 1 (far) -> 0 (perfect)
    public void SetError(float t) {
        t = Mathf.Clamp01(t);
        lr.widthMultiplier = Mathf.Lerp(0.01f, 0.04f, t);
        var c = Color.white; c.a = Mathf.Lerp(1f, 0.3f, 1f - t);
        lr.startColor = lr.endColor = c;
    }

    public void Redraw() {
        var center = transform.position;
        var n = normal.normalized;
        Vector3 t = Vector3.Cross(n, Vector3.right);
        if (t.sqrMagnitude < 1e-6f) t = Vector3.Cross(n, Vector3.forward);
        t.Normalize();
        var b = Vector3.Cross(n, t);

        for (int i = 0; i <= segments; i++) {
            float a = (i / (float)segments) * Mathf.PI * 2f;
            var p = center + (t * Mathf.Cos(a) + b * Mathf.Sin(a)) * radius;
            lr.SetPosition(i, p);
        }
    }
}
