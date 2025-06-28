using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Platformer.UI
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Menu Panels")]
        public GameObject mainMenuPanel;
        public GameObject optionsPanel;
        public GameObject creditsPanel;
        
        [Header("Transition")]
        public CanvasGroup fadeGroup;
        public float fadeSpeed = 1.5f;
        
        [Header("Options")]
        public Slider musicVolumeSlider;
        
        
        private void Start()
        {
            // Start with main menu active
            ShowMainMenu();
            
            // Initialize settings values
            if (musicVolumeSlider != null)
                musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            
            // Ensure time is normal
            Time.timeScale = 1f;
            
            // Fade in when menu starts
            if (fadeGroup != null)
            {
                fadeGroup.alpha = 1f;
                StartCoroutine(FadeIn());
            }
        }
        
        #region Panel Navigation
        
        public void ShowMainMenu()
        {
            PlayButtonSound();
            ActivatePanelOnly(mainMenuPanel);
        }
        
        public void ShowOptions()
        {
            PlayButtonSound();
            ActivatePanelOnly(optionsPanel);
        }
        
        public void ShowCredits()
        {
            Debug.Log("Showing Credits");
            PlayButtonSound();
            ActivatePanelOnly(creditsPanel);
        }
        
        private void ActivatePanelOnly(GameObject panel)
        {
            if (mainMenuPanel) mainMenuPanel.SetActive(panel == mainMenuPanel);
            if (optionsPanel) optionsPanel.SetActive(panel == optionsPanel);
            if (creditsPanel) creditsPanel.SetActive(panel == creditsPanel);
        }
        
        #endregion
        
        #region Game Actions
        
        public void PlayTutorial()
        {
            PlayButtonSound();
            StartCoroutine(LoadGameWithFade("Tutorial"));
        }

        public void PLayLevel1()
        {
            PlayButtonSound();
            StartCoroutine(LoadGameWithFade("Level1"));
        }

        public void PlayLevel2()
        {
            PlayButtonSound();
            StartCoroutine(LoadGameWithFade("Level2"));
        }

        public void PlayLevel3()
        {
            PlayButtonSound();
            StartCoroutine(LoadGameWithFade("Level3"));
        }
        
        public void QuitGame()
        {
            PlayButtonSound();
            StartCoroutine(QuitWithFade());
        }
        
        #endregion
        
        #region Settings
        
        public void SetMusicVolume(float volume)
        {
            PlayerPrefs.SetFloat("MusicVolume", volume);
            PlayerPrefs.Save();
            
            // You would update your actual music volume here
            // For example:
            // AudioManager.Instance.SetMusicVolume(volume);
        }
        
        public void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
        }
        
        #endregion
        
        #region Helper Methods
        
        private void PlayButtonSound()
        {
        }
        
        #endregion
        
        #region Coroutines
        
        private IEnumerator LoadGameWithFade(string gameSceneName = "Level1")
        {
            if (fadeGroup != null)
            {
                // Fade out
                float elapsedTime = 0f;
                while (elapsedTime < fadeSpeed)
                {
                    fadeGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeSpeed);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                fadeGroup.alpha = 1f;
            }
            else
            {
                // If no fade, short delay for button sound to play
                yield return new WaitForSeconds(0.2f);
            }

            Debug.Log("Loading game scene: " + gameSceneName);
            
            // Load the game scene
            SceneManager.LoadScene(gameSceneName);
        }
        
        private IEnumerator QuitWithFade()
        {
            if (fadeGroup != null)
            {
                // Fade out
                float elapsedTime = 0f;
                while (elapsedTime < fadeSpeed)
                {
                    fadeGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeSpeed);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                fadeGroup.alpha = 1f;
            }
            else
            {
                // If no fade, short delay
                yield return new WaitForSeconds(0.2f);
            }
            
            // Quit the application
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        
        private IEnumerator FadeIn()
        {
            // Fade in from black
            float elapsedTime = 0f;
            while (elapsedTime < fadeSpeed)
            {
                fadeGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeSpeed);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            fadeGroup.alpha = 0f;
        }
        
        #endregion
    }
}