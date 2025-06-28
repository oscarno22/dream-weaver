using UnityEngine;

namespace Platformer.Mechanics 
{
    [System.Serializable]
    public class PlayerAction
    {
        public float time;
        public Vector3 position;
        public Vector2 velocity;
        public bool isJumping;
        public bool isFacingRight;
        public string animationState;
        public bool isHoldingBox;
        
        public PlayerAction(float time, Vector3 position, Vector2 velocity, bool isJumping, bool isFacingRight, string animationState, bool holdingBox = false)
        {
            this.time = time;
            this.position = position;
            this.velocity = velocity;
            this.isJumping = isJumping;
            this.isFacingRight = isFacingRight;
            this.animationState = animationState;
            this.isHoldingBox = holdingBox;
        }
    }
}