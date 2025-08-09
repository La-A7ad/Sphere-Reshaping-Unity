using UnityEngine;

public abstract class BaseModel : MonoBehaviour
{
    [Header("Module Configuration")]
    [SerializeField] protected ModuleData moduleData;
    
    [Header("Debugging/Testing")]
    [Tooltip("If set, this will override the module index from GameManager")]
    [SerializeField] protected int inspectorModuleIndex = -1;
    
    [Header("Objects to Disable on Completion")]
    [SerializeField] protected GameObject[] objectsToDisableOnCompletion;
    
    protected int currentModuleIndex;
    protected int completedTasks = 0;
    
    [SerializeField] protected AudioClip moduleCompleteSound;
    [SerializeField] protected AudioSource audioSource;

    protected virtual void Start()
    {
        // Determine which module index to use
        currentModuleIndex = (inspectorModuleIndex >= 0) ? inspectorModuleIndex : GameManager.Instance.currentModuleIndex;
        
        if (moduleData.modules[currentModuleIndex].totalTasks <= 0)
        {
            FindObjectOfType<QuizManager>().ShowQuizForCurrentModule();
        }
        
        audioSource = GetComponent<AudioSource>();
        if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();
    }

    protected void StartModule()
    {
        NarrationEvents.ModuleStart(currentModuleIndex);
    }

    protected void StartTask(int taskIndex)
    {
        NarrationEvents.TaskStart(currentModuleIndex, taskIndex);
    }

    public void CompleteTask(int taskIndex)
    {
        NarrationEvents.TaskComplete(currentModuleIndex, taskIndex);
        completedTasks++;
        
        if (completedTasks >= moduleData.modules[currentModuleIndex].totalTasks)
        {
            DisableCompletionObjects();
            FindObjectOfType<QuizManager>().ShowQuizForCurrentModule();
        }
    }

    public void CompleteModule()
    {
        GameManager.Instance.LoadNextModule();
        NarrationEvents.ModuleComplete(currentModuleIndex);
        if (audioSource) audioSource.PlayOneShot(moduleCompleteSound);
    }

    protected void DisableCompletionObjects()
    {
        if (objectsToDisableOnCompletion == null || objectsToDisableOnCompletion.Length == 0) return;

        foreach (var obj in objectsToDisableOnCompletion)
        {
            if (obj != null) obj.SetActive(false);
        }
    }
}