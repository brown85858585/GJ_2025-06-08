using System;
using UnityEngine;

namespace Player.Interfaces
{
    public interface IInputAdapter
    {
        Vector3 Direction { get; }
        Vector3 Look { get; }
        bool IsAccelerating { get; }
        event Action<bool> OnGameInteract;
        event Action<bool> OnInteract;
        event Action OnSwitchInteract; 
        event Action<bool> OnTest;
        event Action<bool> OnQuests;
        public event Action OnZoomIn;
        public event Action OnZoomOut;
        
       void  SwitchAdapterToMiniGameMode();
       void SwitchAdapterToGlobalMode();
    }
}