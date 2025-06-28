using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Platformer.UI
{
    public class GameSceneManager : MonoBehaviour
    {
        // Singleton pattern
        public static GameSceneManager Instance { get; private set; }
        
        [Header("Transition")]
        public float transitionDuration = 1f;
        public CanvasGroup fadeCanvasGroup;
        
        [Header("Loading Screen")]
        public GameObject loadingScreen;
        public UnityEngine.UI.Slider loadingBar;
        
        private void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            // Initialize
            if (fadeCanvasGroup != null)
                fadeCanvasGroup.alpha = 0;
                
            if (loadingScreen != null)
                loadingScreen.SetActive(false);
        }
        
        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneWithTransition(sceneName));
        }
        
        public void ReloadCurrentScene()
        {
            LoadScene(SceneManager.GetActiveScene().name);
        }
        
        // Load a scene asynchronously with a fade transition
        private IEnumerator LoadSceneWithTransition(string sceneName)
        {
            // Fade out
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.gameObject.SetActive(true);
                float elapsed = 0f;
                while (elapsed < transitionDuration * 0.5f)
                {
                    fadeCanvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / (transitionDuration * 0.5f));
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
                fadeCanvasGroup.alpha = 1f;
            }
            
            // Show loading screen if configured
            if (loadingScreen != null)
                loadingScreen.SetActive(true);
            
            // Start loading the scene asynchronously
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;
            
            // Wait until the scene is almost loaded
            while (asyncLoad.progress < 0.9f)
            {
                if (loadingBar != null)
                    loadingBar.value = asyncLoad.progress;
                    
                yield return null;
            }
            
            // Final loading step
            if (loadingBar != null)
                loadingBar.value = 1f;
                
            // Small delay for visual feedback
            yield return new WaitForSecondsRealtime(0.2f);
            
            // Allow scene activation
            asyncLoad.allowSceneActivation = true;
            
            // Wait until the scene is fully loaded
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            
            // Hide loading screen
            if (loadingScreen != null)
                loadingScreen.SetActive(false);
                
            // Short delay to ensure scene is fully ready
            yield return new WaitForSecondsRealtime(0.1f);
            
            // Fade in
            if (fadeCanvasGroup != null)
            {
                float elapsed = 0f;
                while (elapsed < transitionDuration * 0.5f)
                {
                    fadeCanvasGroup.alpha = Mathf.Lerp(1, 0, elapsed / (transitionDuration * 0.5f));
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
                fadeCanvasGroup.alpha = 0f;
                fadeCanvasGroup.gameObject.SetActive(false);
            }
        }
    }
}