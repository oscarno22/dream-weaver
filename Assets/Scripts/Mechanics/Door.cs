using UnityEngine;
using System.Collections;

namespace Platformer.Mechanics
{
    public class Door : MonoBehaviour
    {
        [Header("Settings")]
        public bool startClosed = true;
        public float openSpeed = 2f;
        public float openDistance = 2f;
        public DoorDirection openDirection = DoorDirection.Up;
        
        [Header("Audio")]
        public AudioClip openSound;
        public AudioClip closeSound;
        
        [Header("Layer Settings")]
        public bool changeLayerWhenOpen = true;
        public string openLayerName = "Default"; // The layer when door is open
        public string closedLayerName = "Ground"; // The layer when door is closed
        
        private Vector3 closedPosition;
        private Vector3 openPosition;
        private bool isOpen = false;
        private Vector3 targetPosition;
        private int openLayerId;
        private int closedLayerId;
        private AudioSource audioSource;
        
        public enum DoorDirection
        {
            Up,
            Down,
            Left,
            Right
        }
        
        void Start()
        {
            // Store starting position
            closedPosition = transform.position;
            
            // Calculate open position based on direction
            openPosition = closedPosition;
            switch (openDirection)
            {
                case DoorDirection.Up:
                    openPosition += new Vector3(0, openDistance, 0);
                    break;
                case DoorDirection.Down:
                    openPosition += new Vector3(0, -openDistance, 0);
                    break;
                case DoorDirection.Left:
                    openPosition += new Vector3(-openDistance, 0, 0);
                    break;
                case DoorDirection.Right:
                    openPosition += new Vector3(openDistance, 0, 0);
                    break;
            }
            
            // Get layer IDs
            openLayerId = LayerMask.NameToLayer(openLayerName);
            closedLayerId = LayerMask.NameToLayer(closedLayerName);
            
            // Set up audio source
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && (openSound != null || closeSound != null))
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Initialize state
            isOpen = !startClosed;
            targetPosition = isOpen ? openPosition : closedPosition;
            transform.position = targetPosition;
            UpdateLayer();
        }
        
        void Update()
        {
            // Smooth movement toward target position
            if (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.Lerp(
                    transform.position, 
                    targetPosition, 
                    Time.deltaTime * openSpeed
                );
            }
        }
        
        public void Activate()
        {
            Open();
        }
        
        public void Deactivate()
        {
            Close();
        }
        
        public void Open()
        {
            if (!isOpen)
            {
                isOpen = true;
                targetPosition = openPosition;
                UpdateLayer();
                PlaySound(openSound);
            }
        }
        
        public void Close()
        {
            if (isOpen)
            {
                isOpen = false;
                targetPosition = closedPosition;
                UpdateLayer();
                PlaySound(closeSound);
            }
        }
        
        public void Toggle()
        {
            if (isOpen)
                Close();
            else
                Open();
        }
        
        private void UpdateLayer()
        {
            if (changeLayerWhenOpen)
            {
                gameObject.layer = isOpen ? openLayerId : closedLayerId;
            }
        }
        
        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
    }
}