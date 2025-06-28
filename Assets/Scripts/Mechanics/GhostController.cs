using System.Collections.Generic;
using UnityEngine;
using Platformer.Mechanics;

namespace Platformer.Mechanics
{
    public class GhostController : MonoBehaviour
    {
        private List<PlayerAction> actions;
        private float loopStartTime;
        private int index = 0;
        
        private SpriteRenderer spriteRenderer;
        private Animator animator;
        private Rigidbody2D rb;
        private Collider2D col;

        [Header("Physics Settings")]
        public bool usePhysics = true;
        public float physicsFidelity = 5f;
        public bool allowDrift = false;
        
        [Header("Visual Effects")]
        public GameObject collisionEffectPrefab;
        public float minImpactForceToTriggerEffect = 2f;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            
            // Make the ghost semi-transparent
            Color color = spriteRenderer.color;
            color.a = 0.7f;
            spriteRenderer.color = color;
            
            // Set up physics
            SetupPhysics();
            
            // Make sure ghost is on the right layer
            gameObject.layer = LayerMask.NameToLayer("Ghost");
        }

        void SetupPhysics()
        {
            if (rb != null)
            {
                if (usePhysics)
                {
                    // Use dynamic body type for physics interactions
                    rb.bodyType = RigidbodyType2D.Dynamic;
                    rb.simulated = true;
                    rb.interpolation = RigidbodyInterpolation2D.Interpolate;
                    rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                    
                    // Don't let ghosts rotate due to physics
                    rb.freezeRotation = true;
                    
                    // Make ghosts slightly less massive than the player
                    rb.mass = 0.8f;
                    
                    // Add a bit of drag to prevent excessive bouncing
                    rb.linearDamping = 0.5f;
                }
                else
                {
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    rb.simulated = false;
                }
            }
        }

        public void Initialize(List<PlayerAction> recording, float startTime)
        {
            actions = recording;
            loopStartTime = startTime;
            index = 0;
            
            // Set initial position if we have actions
            if (actions != null && actions.Count > 0)
            {
                transform.position = actions[0].position;
            }
        }

        void Update()
        {
            if (actions == null || actions.Count == 0 || index >= actions.Count) return;

            float currentTime = Time.time - loopStartTime;
            
            // Find the correct action for the current time
            while (index < actions.Count && actions[index].time <= currentTime)
            {
                ApplyAction(actions[index]);
                index++;
            }
        }
        
        void ApplyAction(PlayerAction action)
        {
            if (usePhysics && rb != null && rb.simulated)
            {
                // For physical interactions with the world, use a combined approach
                
                // First, set velocity (this maintains physical interactions)
                rb.linearVelocity = action.velocity;
                
                // Then do a gentle position correction to avoid drift
                // Calculate how far we've drifted from expected position
                Vector2 positionDrift = (Vector2)action.position - rb.position;
                
                // Only correct if we've drifted too far (prevents jittering)
                if (positionDrift.magnitude > 0.5f)
                {
                    // Use MovePosition for a physics-aware teleport
                    rb.MovePosition(action.position);
                }
                else if (positionDrift.magnitude > 0.1f)
                {
                    // For smaller drifts, add a gentle force toward the correct position
                    rb.AddForce(positionDrift * physicsFidelity, ForceMode2D.Force);
                }
            }
            else
            {
                // Pure replay with no physics
                transform.position = action.position;
            }
            
            // Apply visual effects
            spriteRenderer.flipX = !action.isFacingRight;
            
            // Animation parameters
            UpdateAnimation(action);
        }

        private void UpdateAnimation(PlayerAction action)
        {
            if (animator != null)
            {
                // Check what parameters actually exist in your animator
                if (HasParameter("grounded"))
                    animator.SetBool("grounded", !action.isJumping);
                    
                if (HasParameter("velocityX"))
                    animator.SetFloat("velocityX", action.velocity.x);
                    
                if (HasParameter("velocityY"))
                    animator.SetFloat("velocityY", action.velocity.y);
            }
        }

        // Helper method to safely check if a parameter exists
        private bool HasParameter(string paramName)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == paramName)
                    return true;
            }
            return false;
        }

        // Detect collisions with other physical objects
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Ignore collisions with player (should be handled by layers, but double-check)
            if (collision.gameObject.CompareTag("Player"))
                return;
            
            // Visual feedback for collisions with environment
            if (collisionEffectPrefab != null && 
                collision.relativeVelocity.magnitude > minImpactForceToTriggerEffect)
            {
                // Create a visual effect at the collision point
                Instantiate(
                    collisionEffectPrefab, 
                    collision.GetContact(0).point, 
                    Quaternion.identity
                );
            }
        }
        
        // Handle any edge cases in physics simulation
        private void FixedUpdate()
        {
            // Only needed if using physics
            if (!usePhysics || rb == null || !rb.simulated) return;
            
            // If we fell out of the world
            if (transform.position.y < -20f)
            {
                gameObject.SetActive(false);
            }
        }
    }
}