using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Mechanics;
using Platformer.Model;
using Platformer.Core;
using TMPro;
using UnityEngine.SceneManagement;

namespace Platformer.Mechanics
{
    public class LoopManager : MonoBehaviour
    {
        [Header("Loop Settings")]
        public float loopDuration = 8f;
        public int maxLoops = 5;
        public bool gameEndsAfterMaxLoops = true;

        [Header("References")]
        public GameObject playerPrefab;
        public GameObject ghostPrefab;
        public Transform playerSpawnPoint;
        public TextMeshProUGUI loopCounterText;
        public TextMeshProUGUI timerText;

        private float loopStartTime;
        private PlayerController currentPlayer;
        private List<List<PlayerAction>> pastRecordings = new List<List<PlayerAction>>();
        private List<GhostController> activeGhosts = new List<GhostController>();
        private int currentLoopIndex = 0;
        private bool isGameActive = true;

        [Header("End Game Settings")]
        public string mainMenuSceneName = "MainMenu"; // Add scene name for your main menu
        public bool returnToMainMenu = true; // Toggle between returning to menu or quitting

        private void ConfigureGhost(GameObject ghostObject)
        {
            // Make sure it's on the right layer
            ghostObject.layer = LayerMask.NameToLayer("Ghost");

            // Check components
            var rb = ghostObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                rb.freezeRotation = true;
            }

            // Check all child objects are also on the Ghost layer
            foreach (Transform child in ghostObject.transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Ghost");
            }
        }

        void Start()
        {
            // Get references
            var model = Simulation.GetModel<PlatformerModel>();
            currentPlayer = model.player;

            if (playerSpawnPoint == null)
            {
                if (model.spawnPoint != null)
                    playerSpawnPoint = model.spawnPoint.transform;
                else
                    Debug.LogError("No spawn point assigned to LoopManager!");
            }
            
            currentLoopIndex = 0;
            StartNewLoop();
            UpdateUI();
        }

        void Update()
        {
            if (!isGameActive) return;
            
            float timeRemaining = loopDuration - (Time.time - loopStartTime);
            
            // Update timer UI
            if (timerText != null)
            {
                timerText.text = $"Time: {timeRemaining:0.0}";
            }

            // Check if loop is complete
            if (timeRemaining <= 0)
            {
                EndLoop();
                currentLoopIndex++;
                
                if (currentLoopIndex >= maxLoops)
                {
                    if (gameEndsAfterMaxLoops)
                        EndGame();
                    else
                        ResetGame();
                }
                else
                {
                    StartNewLoop();
                }
                
                UpdateUI();
            }
        }

        void StartNewLoop()
        {
            loopStartTime = Time.time;
            
            // Reset player position
            if (currentPlayer != null && playerSpawnPoint != null)
            {
                currentPlayer.Teleport(playerSpawnPoint.position);
                currentPlayer.StartRecording(loopStartTime);
            }
            else
            {
                Debug.LogError("Missing player or spawn point reference!");
            }
            
            // Spawn ghosts for all past recordings
            SpawnGhosts();
        }

        void EndLoop()
        {
            if (currentPlayer != null)
            {
                var recording = currentPlayer.StopRecording();
                pastRecordings.Add(recording);
            }
            
            // Clean up old ghosts
            foreach(var ghost in activeGhosts)
            {
                if (ghost != null)
                    Destroy(ghost.gameObject);
            }
            activeGhosts.Clear();
        }

        void SpawnGhosts()
        {
            for (int i = 0; i < pastRecordings.Count; i++)
            {
                if (ghostPrefab != null && playerSpawnPoint != null)
                {
                    var ghost = Instantiate(ghostPrefab, playerSpawnPoint.position, Quaternion.identity);
                    ConfigureGhost(ghost);
                    var controller = ghost.GetComponent<GhostController>();
                    
                    if (controller != null)
                    {
                        controller.Initialize(pastRecordings[i], loopStartTime);
                        activeGhosts.Add(controller);
                        
                        // Optional: Color ghosts differently based on how old they are
                        SpriteRenderer renderer = ghost.GetComponent<SpriteRenderer>();
                        if (renderer != null)
                        {
                            float alpha = 0.7f - (0.1f * i); // Older ghosts are more transparent
                            Color color = renderer.color;
                            color.a = Mathf.Clamp(alpha, 0.2f, 0.7f);
                            renderer.color = color;
                        }
                    }
                }
            }
        }

        void UpdateUI()
        {
            if (loopCounterText != null)
            {
                loopCounterText.text = $"Loop: {currentLoopIndex + 1}/{maxLoops}";
            }
        }

        void EndGame()
        {
            Debug.Log("Maximum loops reached! Game Over.");
            isGameActive = false;
            
            // This is where you'd show a game complete screen, stats, etc.
            
            // After a delay, either return to main menu or quit
            StartCoroutine(DelayedGameEnd(3f));
        }

        IEnumerator DelayedGameEnd(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (returnToMainMenu)
            {
                // Load the main menu scene
                SceneManager.LoadScene(mainMenuSceneName);
            }
            else
            {
                // Quit the application (only works in built game, not in editor)
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
            }
        }

        IEnumerator DelayedReset(float delay)
        {
            yield return new WaitForSeconds(delay);
            ResetGame();
        }

        void ResetGame()
        {
            // Reset everything
            foreach(var ghost in activeGhosts)
            {
                if (ghost != null)
                    Destroy(ghost.gameObject);
            }
            
            activeGhosts.Clear();
            pastRecordings.Clear();
            currentLoopIndex = 0;
            isGameActive = true;
            
            // Teleport player back to start
            if (currentPlayer != null && playerSpawnPoint != null)
            {
                currentPlayer.Teleport(playerSpawnPoint.position);
            }
            
            // Start a new first loop
            StartNewLoop();
            UpdateUI();
        }

        // Public method to allow forcing a reset from elsewhere
        public void ForceReset()
        {
            ResetGame();
        }
        
        // Optional: Public method to add time to current loop
        public void AddTime(float seconds)
        {
            loopStartTime -= seconds; // Subtracting from start time effectively adds time
        }
    }
}