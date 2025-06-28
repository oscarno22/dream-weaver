using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Platformer.Mechanics
{
    public class MenuTransitionTrigger : MonoBehaviour
    {
        [Header("Settings")]
        public string menuSceneName = "MainMenu";
        public float transitionDelay = 0.5f;
        public bool triggerOnEnter = true;
        
        [Header("Optional Effects")]
        public bool fadeScreen = true;
        public AudioClip transitionSound;
        
        private bool isTriggered = false;
        
        private void OnTriggerEnter2D(Collider2D other) 
        {
            if (isTriggered || !triggerOnEnter) return;
            
            // Check if it's the player
            if (other.CompareTag("Player")) 
            {
                TriggerMenuTransition();
            }
        }
        
        private void OnCollisionEnter2D(Collision2D other) 
        {
            if (isTriggered || triggerOnEnter) return;
            
            // Check if it's the player
            if (other.gameObject.CompareTag("Player")) 
            {
                TriggerMenuTransition();
            }
        }
        
        private void TriggerMenuTransition() 
        {
            if (isTriggered) return;
            isTriggered = true;
            
            // Play sound if assigned
            if (transitionSound != null) 
            {
                AudioSource.PlayClipAtPoint(transitionSound, transform.position);
            }
            
            // Start transition coroutine
            StartCoroutine(TransitionToMenu());
        }
        
        private IEnumerator TransitionToMenu() 
        {
            // Here you could add screen fade or other transition effects
            if (fadeScreen) 
            {
                // Simple example - in a real game you'd use a proper UI fade
                Debug.Log("Fading screen...");
                // You might want to instantiate a UI canvas with fade animation here
            }
            
            // Wait for delay
            yield return new WaitForSeconds(transitionDelay);
            
            // Load menu scene
            SceneManager.LoadScene(menuSceneName);
        }
        
        // Public method to manually trigger the transition
        public void ManualTrigger() 
        {
            TriggerMenuTransition();
        }
    }
}