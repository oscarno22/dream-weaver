using UnityEngine;

namespace Platformer.Mechanics
{
    // This script ensures that required layers exist for the ghost system
    // Add it to any GameObject in your scene (like the LoopManager)
    public class LayerManager : MonoBehaviour
    {
        void Awake()
        {
            // Check if Ghost layer exists, if not, warn user
            if (LayerMask.NameToLayer("Ghost") == -1)
            {
                Debug.LogWarning("Ghost layer not found! Please create a 'Ghost' layer in Edit > Project Settings > Tags and Layers");
                Debug.LogWarning("Also set up the Physics2D collision matrix so Ghosts collide with the environment but not with players");
            }
        }
    }
}