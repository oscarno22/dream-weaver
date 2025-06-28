using UnityEngine;
using System.Collections.Generic;

namespace Platformer.Mechanics
{
    public class MultiSwitchDoor : MonoBehaviour
    {
        [Header("Connected Elements")]
        public PressurePlate[] requiredSwitches;
        public Door controlledDoor;
        
        [Header("Settings")]
        public bool requireAllSwitches = true;
        [Tooltip("How many switches need to be active if not requiring all of them")]
        public int requiredSwitchCount = 2;
        
        private void Update()
        {
            int activeSwitches = 0;
            
            // Count active switches
            foreach (PressurePlate plate in requiredSwitches)
            {
                if (plate != null && plate.IsActivated)
                {
                    activeSwitches++;
                }
            }
            
            // Determine if we should open the door
            bool shouldOpen = requireAllSwitches 
                ? (activeSwitches == requiredSwitches.Length) 
                : (activeSwitches >= requiredSwitchCount);
                
            // Open or close the door based on switch state
            if (shouldOpen)
                controlledDoor.Open();
            else
                controlledDoor.Close();
        }
    }
}