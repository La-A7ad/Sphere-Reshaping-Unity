using UnityEngine;

namespace SphereReshaper.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        public int currentModuleIndex = 0;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void LoadNextModule()
        {
            // Placeholder: Implement module loading logic
            Debug.Log("Loading next module...");
        }
    }
}