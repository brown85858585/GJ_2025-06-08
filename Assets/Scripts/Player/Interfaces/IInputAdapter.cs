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
        event Action OnPutItemDown; 
        event Action<bool> OnTest;
        event Action<bool> OnQuests;
    }
}