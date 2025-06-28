using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

namespace Platformer.UI
{
    public class QuitGame : MonoBehaviour
    {
        [SerializeField] private CanvasGroup fadeGroup;
        [SerializeField] private float fadeSpeed = 1.0f;

        public void PlayButtonSound()
        {
            // Placeholder for button sound logic
            Debug.Log("Button sound played");
        }

        public void QuitTheGame()
        {
            PlayButtonSound();
            StartCoroutine(QuitWithFade());
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
    }
}