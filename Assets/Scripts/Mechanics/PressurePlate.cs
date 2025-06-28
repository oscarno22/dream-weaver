using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Platformer.Mechanics
{
    public class PressurePlate : MonoBehaviour
    {
        [Header("Settings")]
        public float activationTime = 3f;  // How long the switch stays pressed
        public bool staysPressed = false;  // If true, stays pressed permanently once triggered
        public bool requiresConstantPressure = true;  // If true, needs someone standing on it
        
        [Header("Visual Feedback")]
        public Color activatedColor = new Color(0.2f, 0.8f, 0.2f);
        public Color deactivatedColor = new Color(0.8f, 0.2f, 0.2f);
        public float visualHeight = 0.1f;  // How much the switch visually "presses down"
        public Transform visualElement;  // The part that moves down when pressed
        
        [Header("References")]
        public Door[] connectedDoors;  // Doors this switch controls
        
        private SpriteRenderer spriteRenderer;
        private float timeReleased;
        private Vector3 originalPosition;
        private Vector3 pressedPosition;
        private int objectsOnPlate = 0;
        private bool isActivated = false;
        public bool IsActivated => isActivated;
        
        void Start()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            
            // If no visual element assigned, try to use this object
            if (visualElement == null)
                visualElement = transform;
                
            // Set up positions
            originalPosition = visualElement.localPosition;
            pressedPosition = originalPosition - new Vector3(0, visualHeight, 0);
            
            // Set initial color
            UpdateVisuals();
        }
        
        void Update()
        {
            if (!staysPressed && !requiresConstantPressure && isActivated)
            {
                // Check timer for auto-release
                if (Time.time - timeReleased > activationTime)
                {
                    Deactivate();
                }
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Check if this is a player, ghost, or pickup box
            if (other.CompareTag("Player") || other.CompareTag("PickupBox") || other.gameObject.layer == LayerMask.NameToLayer("Ghost"))
            {
                objectsOnPlate++;
                Activate();
            }
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            // Check if this is a player, ghost, or pickup box
            if (other.CompareTag("Player") || other.CompareTag("PickupBox") || other.gameObject.layer == LayerMask.NameToLayer("Ghost"))
            {
                objectsOnPlate--;
                
                // If we require constant pressure and nothing is on the plate
                if (requiresConstantPressure && objectsOnPlate <= 0)
                {
                    Deactivate();
                }
                else if (!requiresConstantPressure)
                {
                    // Start the timer for auto-release
                    timeReleased = Time.time;
                }
            }
        }
        
        public void Activate()
        {
            if (!isActivated || requiresConstantPressure)
            {
                isActivated = true;
                
                // Notify all connected doors
                foreach (Door door in connectedDoors)
                {
                    if (door != null)
                        door.Activate();
                }
                
                // Update visuals
                UpdateVisuals();
            }
        }
        
        public void Deactivate()
        {
            if (staysPressed) return;  // Can't deactivate if set to stay pressed
            
            isActivated = false;
            objectsOnPlate = 0; // Reset count if we somehow got misaligned
            
            // Notify all connected doors
            foreach (Door door in connectedDoors)
            {
                if (door != null)
                    door.Deactivate();
            }
            
            // Update visuals
            UpdateVisuals();
        }
        
        private void UpdateVisuals()
        {
            // Update color
            if (spriteRenderer != null)
            {
                spriteRenderer.color = isActivated ? activatedColor : deactivatedColor;
            }
            
            // Update position
            if (visualElement != null)
            {
                visualElement.localPosition = isActivated ? pressedPosition : originalPosition;
            }
        }
    }
}