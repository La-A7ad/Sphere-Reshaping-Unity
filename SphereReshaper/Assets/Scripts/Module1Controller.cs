using System.Collections;
using UnityEngine;

public class Module1Controller : BaseModel
{
    [Header("Celestial Bodies")]
    public GameObject sun;
    public GameObject earth;
    public GameObject moon;

    [Header("Visual Guides")]
    public GameObject sunGuide;
    public GameObject earthGuide;
    public GameObject moonGuide;

    [Header("Narration Delays")]
    public float initialDelay = 1f;
    public float afterShapeTaskDelay = 2f;
    public float afterSizeTaskDelay = 2f;

    [Header("Interaction Settings")]
    public float shapeSmoothingSpeed = 0.1f;
    public float sizeScalingSpeed = 0.5f;

    private bool isShapeTaskActive = false;
    private bool isSizeTaskActive = false;
    private Vector3 originalSunScale;
    private Vector3 originalEarthScale;
    private Vector3 originalMoonScale;

    protected override void Start()
    {
        base.Start();

        // Store original scales
        originalSunScale = sun.transform.localScale;
        originalEarthScale = earth.transform.localScale;
        originalMoonScale = moon.transform.localScale;

        // Initialize objects for first task
        InitializeObjectsForShapeTask();

        // Start module sequence
        StartCoroutine(ModuleStartSequence());
    }

    IEnumerator ModuleStartSequence()
    {
        yield return new WaitForSeconds(initialDelay);
        StartModule();
        StartCoroutine(PlayTask1Narration());
    }

    void InitializeObjectsForShapeTask()
    {
        // Flatten the spheres for the initial task
        sun.transform.localScale = new Vector3(originalSunScale.x, originalSunScale.y * 0.3f, originalSunScale.z);
        earth.transform.localScale = new Vector3(originalEarthScale.x, originalEarthScale.y * 0.3f, originalEarthScale.z);
        moon.transform.localScale = new Vector3(originalMoonScale.x, originalMoonScale.y * 0.3f, originalMoonScale.z);

        // Hide guides initially
        sunGuide.SetActive(false);
        earthGuide.SetActive(false);
        moonGuide.SetActive(false);
    }

    IEnumerator PlayTask1Narration()
    {
        yield return new WaitForSeconds(12f); // Wait for initial narration to complete
        StartShapeCorrectionTask();
    }

    void StartShapeCorrectionTask()
    {
        StartTask(0); // Notify base system we're starting task 0
        isShapeTaskActive = true;

        // Show visual guides
        sunGuide.SetActive(true);
        earthGuide.SetActive(true);
        moonGuide.SetActive(true);
    }

    IEnumerator PlayTask2Narration()
    {
        yield return new WaitForSeconds(5f); // Wait after shape task completion
        StartSizeScalingTask();
    }

    void StartSizeScalingTask()
    {
        StartTask(1); // Notify base system we're starting task 1
        isShapeTaskActive = false;
        isSizeTaskActive = true;

        // Hide shape guides
        sunGuide.SetActive(false);
        earthGuide.SetActive(false);
        moonGuide.SetActive(false);

        // Reset scales to spherical but wrong sizes
        sun.transform.localScale = originalSunScale * 0.2f; // Too small
        earth.transform.localScale = originalEarthScale;    // Reference size
        moon.transform.localScale = originalMoonScale * 2f; // Too big
    }

    void Update()
    {
        if (isShapeTaskActive)
        {
            HandleShapeCorrection();
        }
        else if (isSizeTaskActive)
        {
            HandleSizeScaling();
        }
    }

    void HandleShapeCorrection()
    {
        bool allSpheresCorrected = true;

        // Check for mouse input and correct shapes
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                GameObject hitObject = hit.collider.gameObject;

                // Smoothly adjust the shape towards original scale
                Vector3 newScale = hitObject.transform.localScale;
                Vector3 targetScale = hitObject == sun ? originalSunScale :
                                     (hitObject == earth ? originalEarthScale : originalMoonScale);

                newScale.y = Mathf.Lerp(newScale.y, targetScale.y, shapeSmoothingSpeed * Time.deltaTime);
                hitObject.transform.localScale = newScale;
            }
        }

        // Check if all objects are spherical enough (close to original scale)
        if (sun.transform.localScale.y < originalSunScale.y * 0.95f ||
            earth.transform.localScale.y < originalEarthScale.y * 0.95f ||
            moon.transform.localScale.y < originalMoonScale.y * 0.95f)
        {
            allSpheresCorrected = false;
        }

        // If all shapes are corrected, complete the task
        if (allSpheresCorrected)
        {
            isShapeTaskActive = false;
            Task1Completed();
        }
    }

    public void Task1Completed()
    {
        Debug.Log("Module 1 Task 1 Completed");
        CompleteTask(0); // Notify base system
        StartCoroutine(PlayTask2Narration());
    }

    void HandleSizeScaling()
    {
        bool sizesCorrect = false;

        // Check for mouse wheel input to resize objects
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                GameObject hitObject = hit.collider.gameObject;
                Vector3 scaleChange = Vector3.one * scroll * sizeScalingSpeed;

                // Scale the object
                hitObject.transform.localScale += scaleChange;

                // Prevent scaling below minimum size
                if (hitObject.transform.localScale.x < 0.1f)
                {
                    hitObject.transform.localScale = Vector3.one * 0.1f;
                }
            }
        }

        // Check if sizes are approximately correct
        float moonSizeRatio = moon.transform.localScale.x / earth.transform.localScale.x;
        float sunSizeRatio = sun.transform.localScale.x / earth.transform.localScale.x;

        // Using forgiving ratios for user experience
        // Moon should be ~1/4 Earth size, Sun should be ~110x Earth size
        if (moonSizeRatio > 0.2f && moonSizeRatio < 0.3f &&
            sunSizeRatio > 50f && sunSizeRatio < 150f)
        {
            sizesCorrect = true;
        }

        // If sizes are correct, complete the task
        if (sizesCorrect)
        {
            isSizeTaskActive = false;
            Task2Completed();
        }
    }

    public void Task2Completed()
    {
        Debug.Log("Module 1 Task 2 Completed");
        CompleteTask(1); // Notify base system

        // Complete the module
        CompleteModule();
    }
}
