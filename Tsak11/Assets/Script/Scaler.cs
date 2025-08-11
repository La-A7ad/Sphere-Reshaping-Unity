using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class Scaler : MonoBehaviour
{
    [Header("Planets")]
    [SerializeField] public Transform Sun;
    [SerializeField] public Transform Earth;
    [SerializeField] public Transform Moon;

    [Header("Rings (optional)")]
    [SerializeField] private RingDrawer sunRing;
    [SerializeField] private RingDrawer moonRing;

    [Header("Refs")]
    [SerializeField] private GameManager gameManager;

    [Header("Selection")]
    [SerializeField] private LayerMask selectableLayers = ~0;

    [Header("Scaling")]
    [SerializeField] private float dragSensitivity = 0.0025f;
    [SerializeField] private float minAxis = 0.1f;
    [SerializeField] private float snapMargin = 0.0f;

    private Transform selectedObject;
    private Vector3 lastMousePosition;
    private bool isDragging;

    

  
    private Transform ResolveRoot(Transform t) => t ? t.root : null;

    [SerializeField] bool debugForcePhase2 = false;
    private bool IsPhase2 => debugForcePhase2 || (gameManager && gameManager.phase == GameManager.Phase.UniformResize);


    private bool IsAllowedUniformTarget(Transform root)
    {
        if (!IsPhase2) return false;
        return root == Sun || root == Moon ||
               (root && (root.CompareTag("Sun") || root.CompareTag("Moon")));
    }

    private RingDrawer RingFor(Transform root)
    {
        if (!root) return null;
        if (root == Sun && sunRing) return sunRing;
        if (root == Moon && moonRing) return moonRing;
        return null;
    }

    private void Update()
    {
        if (!enabled) return;

        if (!IsPhase2) {
            if (isDragging) isDragging = false;
            return;
        }

        if (Input.GetMouseButtonDown(0)) TryBeginDrag();

        if (isDragging && selectedObject)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            float m = 1f + (delta.y * dragSensitivity);
            if (m < 0.01f) m = 0.01f;

            Vector3 s = selectedObject.localScale * m;
            s.x = Mathf.Max(minAxis, s.x);
            s.y = Mathf.Max(minAxis, s.y);
            s.z = Mathf.Max(minAxis, s.z);

            if (snapMargin > 0f)
            {
                float currentRadius = Mathf.Max(s.x, s.y, s.z) * 0.5f;
                var ring = RingFor(selectedObject);
                if (ring != null)
                {
                    float targetR = ring.TargetRadius;
                    if (Mathf.Abs(currentRadius - targetR) <= snapMargin)
                    {
                        float wantedDiameter = targetR * 2f;
                        float factor = wantedDiameter / Mathf.Max(s.x, s.y, s.z);
                        s *= factor;
                    }
                }
            }

            selectedObject.localScale = s;
            lastMousePosition = Input.mousePosition;

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                Debug.Log("[Scaler] Drag end.");
            }
        }

        if (Input.GetMouseButtonUp(0)) isDragging = false;
    }

    private void TryBeginDrag()
    {
        if (!IsPhase2) { Debug.Log("[Scaler] Not in Phase 2; selection blocked."); return; }

        if (!IsPhase2) return;
        if (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) return;

        Camera cam = Camera.main;
        if (!cam)
        {
            Debug.LogWarning("[Scaler] No MainCamera found.");
            return;
        }

        if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 5000f, selectableLayers))
        {
            Transform root = ResolveRoot(hit.transform);
            if (!IsAllowedUniformTarget(root))
            {
                Debug.Log("[Scaler] Hit not allowed in Phase 2: " + (root ? root.name : "null"));
                return;
            }

            selectedObject = root;
            lastMousePosition = Input.mousePosition;
            isDragging = true;
            Debug.Log("[Scaler] Selected " + selectedObject.name);
        }
        else Debug.Log("[Scaler] Raycast missed.");
    }
}