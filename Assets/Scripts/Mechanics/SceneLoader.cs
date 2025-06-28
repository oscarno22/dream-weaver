using UnityEngine;
using UnityEngine.SceneManagement;

namespace Platformer.UI
{
    public class SceneLoader : MonoBehaviour
    {
        [Tooltip("Optional transition effect time in seconds")]
        public float transitionTime = 0.5f;
        
        [Tooltip("Optional fade effect")]
        public bool useFadeEffect = false;
        public CanvasGroup fadeCanvasGroup;
        
        // Singleton pattern to access the SceneLoader from anywhere
        public static SceneLoader Instance { get; private set; }

        private void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Optional: keep between scenes
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Method to be called from UI button OnClick events
        public void LoadScene(string sceneName)
        {
            if (useFadeEffect && fadeCanvasGroup != null)
            {
                StartCoroutine(FadeAndLoadScene(sceneName));
            }
            else
            {
                // Load the scene immediately
                SceneManager.LoadScene(sceneName);
            }
        }

        // Specific method for main menu
        public void LoadMainMenu()
        {
            LoadScene("MainMenu");
        }
        
        private System.Collections.IEnumerator FadeAndLoadScene(string sceneName)
        {
            // Ensure the CanvasGroup is visible and at the front
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.gameObject.SetActive(true);
            
            // Fade to black
            float elapsedTime = 0;
            while (elapsedTime < transitionTime)
            {
                fadeCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / transitionTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            fadeCanvasGroup.alpha = 1f;
            
            // Load the scene
            SceneManager.LoadScene(sceneName);
        }
    }
}
