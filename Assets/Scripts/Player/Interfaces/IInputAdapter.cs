using System;
using UnityEngine;

namespace Player.Interfaces
{
    public interface IInputAdapter
    {
        Vector3 Direction { get; }
        Vector3 Look { get; }
        bool IsAccelerating { get; }
        event Action<bool> OnInteract;
        event Action OnSwitchInteract; 
        event Action<bool> OnTest;
        event Action<bool> OnQuests;
        public event Action OnZoomIn;
        public event Action OnZoomOut;
        
        // MiniGame Actions
        event Action<bool> OnGameInteract;
        
       void  SwitchAdapterToMiniGameMode();
       void SwitchAdapterToGlobalMode();
    }
}