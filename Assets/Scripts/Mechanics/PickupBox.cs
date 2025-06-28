using UnityEngine;
using Platformer.Mechanics;

namespace Platformer.Mechanics
{
    public class PickupBox : MonoBehaviour
    {
        public float weight = 1f;
        public bool canBePickedUp = true;
        
        [Header("Optional")]
        public SpriteRenderer highlightSprite;
        public AudioClip pickupSound;
        public AudioClip dropSound;
        
        private void Start()
        {
            // Make sure the box has the right tag
            if (gameObject.tag != "PickupBox")
                gameObject.tag = "PickupBox";
                
            // Make sure it has a collider and rigidbody
            if (GetComponent<Collider2D>() == null)
                gameObject.AddComponent<BoxCollider2D>();
                
            if (GetComponent<Rigidbody2D>() == null)
            {
                Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
                rb.freezeRotation = true;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }
        }
        
        // Optional: Visual feedback when player is nearby
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && highlightSprite != null)
            {
                highlightSprite.enabled = true;
            }
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player") && highlightSprite != null)
            {
                highlightSprite.enabled = false;
            }
        }
    }
}