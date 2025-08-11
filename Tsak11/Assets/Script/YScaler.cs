using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class YScaler : MonoBehaviour
{
    [Header("Planets")]
    [SerializeField] public Transform Sun;
    [SerializeField] public Transform Earth;
    [SerializeField] public Transform Moon;

    [Header("Selection")]
    [SerializeField] private LayerMask selectableLayers = ~0;
    [SerializeField] private bool allowTagFallback = true;

    [Header("Scaling")]
    [SerializeField] private float dragSensitivity = 0.0025f;
    [SerializeField] private float minAxis = 0.1f;

    private Transform selectedObject;
    private Vector3 lastMousePosition;
    private bool isDragging;

    private void Update()
    {
        if (!enabled) return;

        if (Input.GetMouseButtonDown(0)) TryBeginDrag();

        if (isDragging && selectedObject)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            float yDelta = delta.y * dragSensitivity;

            Vector3 s = selectedObject.localScale;
            float maxXZ = Mathf.Max(s.x, s.z);
            s.y = Mathf.Clamp(s.y + yDelta, minAxis, maxXZ);
            selectedObject.localScale = s;

            lastMousePosition = Input.mousePosition;

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                Debug.Log("[YScaler] Drag end.");
            }
        }

        if (Input.GetMouseButtonUp(0)) isDragging = false;
    }

    private void TryBeginDrag()
    {
        if (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) return;

        Camera cam = Camera.main;
        if (!cam)
        {
            Debug.LogWarning("[YScaler] No MainCamera found.");
            return;
        }

        if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 5000f, selectableLayers))
        {
            Transform root = hit.transform.root;
            if (!IsPlanet(root))
            {
                Debug.Log($"[YScaler] Hit non-planet: {root?.name ?? "null"}");
                return;
            }

            selectedObject = root;
            lastMousePosition = Input.mousePosition;
            isDragging = true;
            Debug.Log($"[YScaler] Selected {selectedObject.name}");
        }
        else Debug.Log("[YScaler] Raycast missed.");
    }

    private bool IsPlanet(Transform candidate)
    {
        if (!candidate) return false;
        if (candidate == Sun || candidate == Earth || candidate == Moon) return true;
        if (allowTagFallback && (candidate.CompareTag("Sun") || candidate.CompareTag("Earth") || candidate.CompareTag("Moon"))) return true;
        return false;
    }
}
