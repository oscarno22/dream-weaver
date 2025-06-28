using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
using UnityEngine.InputSystem;

/// <summary>
/// Shared structure for recording time loop actions.
/// </summary>

namespace Platformer.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController : KinematicObject
    {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        public float maxSpeed = 7;
        public float jumpTakeOffSpeed = 7;

        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;

        public Collider2D collider2d;
        public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;

        bool jump;
        Vector2 move;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        private InputAction m_MoveAction;
        private InputAction m_JumpAction;

        public Bounds Bounds => collider2d.bounds;

        // ---------- ADDED FOR TIME LOOP ----------
        private List<PlayerAction> actionRecording;
        private float recordingStartTime;
        public bool isRecording = false;
        // -----------------------------------------

        // Box pickup variables
        [Header("Pickup Settings")]
        public Transform holdPoint;
        public float pickupRange = 1f;
        public LayerMask pickupLayerMask;
        private InputAction m_PickupAction;
        private GameObject heldBox;
        private bool isHoldingBox = false;
        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            m_MoveAction = InputSystem.actions.FindAction("Player/Move");
            m_JumpAction = InputSystem.actions.FindAction("Player/Jump");
            m_PickupAction = InputSystem.actions.FindAction("Player/Pickup");

            m_MoveAction.Enable();
            m_JumpAction.Enable();
            m_PickupAction.Enable();

            // ---------- ADDED ----------
            actionRecording = new List<PlayerAction>();
            // ---------------------------
        }

        protected override void Update()
        {
            if (controlEnabled)
            {
                move.x = m_MoveAction.ReadValue<Vector2>().x;

                if (jumpState == JumpState.Grounded && m_JumpAction.WasPressedThisFrame())
                    jumpState = JumpState.PrepareToJump;
                else if (m_JumpAction.WasReleasedThisFrame())
                {
                    stopJump = true;
                    Schedule<PlayerStopJump>().player = this;
                }
            }
            else
            {
                move.x = 0;
            }

            UpdateJumpState();

            // Handle pickup/drop
            if (controlEnabled && m_PickupAction.WasPressedThisFrame())
            {
                if (!isHoldingBox)
                    TryPickup();
                else
                    DropBox();
            }

            // ---------- ADDED: record player position & input ----------
            if (isRecording)
            {
                // Record the current state
                PlayerAction action = new PlayerAction(
                    Time.time - recordingStartTime,
                    transform.position,
                    velocity, 
                    jumpState == JumpState.InFlight,
                    !spriteRenderer.flipX,
                    animator.GetCurrentAnimatorStateInfo(0).shortNameHash.ToString(),
                    isHoldingBox
                );
                actionRecording.Add(action);
            }
            // -----------------------------------------------------------

            base.Update();
        }

        void TryPickup()
        {
            // Check for box in front of the player
            Vector2 direction = !spriteRenderer.flipX ? Vector2.right : Vector2.left;
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position, 
                direction, 
                pickupRange, 
                pickupLayerMask
            );

            if (hit.collider != null && hit.collider.CompareTag("PickupBox"))
            {
                GameObject box = hit.collider.gameObject;
                PickupBox(box);
            }
        }

        void PickupBox(GameObject box)
        {
            // Store reference to box
            heldBox = box;
            
            // Disable box physics
            Rigidbody2D boxRb = box.GetComponent<Rigidbody2D>();
            if (boxRb != null)
            {
                boxRb.simulated = false;
            }
            
            // Disable box collider
            Collider2D boxCollider = box.GetComponent<Collider2D>();
            if (boxCollider != null)
            {
                boxCollider.enabled = false;
            }
            
            // Parent box to hold point
            box.transform.SetParent(holdPoint);
            box.transform.localPosition = Vector3.zero;
            box.transform.localRotation = Quaternion.identity;
            
            isHoldingBox = true;
            
            // Optional: Adjust player speed when holding box
            maxSpeed *= 0.8f;
            
            // Optional: Play pickup sound
            if (audioSource != null)
            {
                // audioSource.PlayOneShot(pickupSound);
            }
        }

        void DropBox()
        {
            if (heldBox != null)
            {
                // Re-enable physics
                Rigidbody2D boxRb = heldBox.GetComponent<Rigidbody2D>();
                if (boxRb != null)
                {
                    boxRb.simulated = true;
                }
                
                // Re-enable collider
                Collider2D boxCollider = heldBox.GetComponent<Collider2D>();
                if (boxCollider != null)
                {
                    boxCollider.enabled = true;
                }
                
                // Unparent from player
                heldBox.transform.SetParent(null);
                
                // Optional: Apply small force in the direction player is facing
                if (boxRb != null)
                {
                    Vector2 dropDirection = !spriteRenderer.flipX ? Vector2.right : Vector2.left;
                    boxRb.linearVelocity = new Vector2(dropDirection.x * 2f, 1f);
                }
                
                heldBox = null;
                isHoldingBox = false;
                
                // Reset player speed
                maxSpeed /= 0.8f;
                
                // Optional: Play drop sound
                if (audioSource != null)
                {
                    // audioSource.PlayOneShot(dropSound);
                }
            }
        }

        // Clear held box when player dies or loop resets
        public void ClearHeldBox()
        {
            if (isHoldingBox && heldBox != null)
            {
                heldBox.transform.SetParent(null);
                
                Rigidbody2D boxRb = heldBox.GetComponent<Rigidbody2D>();
                if (boxRb != null)
                    boxRb.simulated = true;
                    
                Collider2D boxCollider = heldBox.GetComponent<Collider2D>();
                if (boxCollider != null)
                    boxCollider.enabled = true;
                
                heldBox = null;
                isHoldingBox = false;
                maxSpeed /= 0.8f;
            }
        }

        void UpdateJumpState()
        {
            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            if (jump && IsGrounded)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            if (move.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (move.x < -0.01f)
                spriteRenderer.flipX = true;

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            targetVelocity = move * maxSpeed;
        }

        // ---------- ADDED: for time loop integration ----------
        public void StartRecording(float loopStart)
        {
            recordingStartTime = loopStart;
            actionRecording = new List<PlayerAction>();
            isRecording = true;
        }

        public List<PlayerAction> StopRecording()
        {
            isRecording = false;
            return actionRecording;
        }

        public new void Teleport(Vector3 position)
        {
            base.Teleport(position);  // This calls KinematicObject.Teleport which sets position and zeros velocity
            jumpState = JumpState.Grounded;  // Reset jump state specific to PlayerController
        }
        // -------------------------------------------------------

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}
