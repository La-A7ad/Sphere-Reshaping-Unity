namespace SphereReshaper.Core
{
    public static class NarrationEvents
    {
        public static void ModuleStart(int moduleIndex)
        {
            // Placeholder: Implement narration event
            UnityEngine.Debug.Log($"Module {moduleIndex} started.");
        }

        public static void TaskStart(int moduleIndex, int taskIndex)
        {
            // Placeholder: Implement narration event
            UnityEngine.Debug.Log($"Task {taskIndex} of module {moduleIndex} started.");
        }

        public static void TaskComplete(int moduleIndex, int taskIndex)
        {
            // Placeholder: Implement narration event
            UnityEngine.Debug.Log($"Task {taskIndex} of module {moduleIndex} completed.");
        }

        public static void ModuleComplete(int moduleIndex)
        {
            // Placeholder: Implement narration event
            UnityEngine.Debug.Log($"Module {moduleIndex} completed.");
        }
    }
}
