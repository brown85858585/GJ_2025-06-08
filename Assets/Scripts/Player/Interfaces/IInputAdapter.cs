using System;
using UnityEngine;

namespace Player.Interfaces
{
    public interface IInputAdapter
    {
        event Action<Vector3> OnMove;
        Vector3 Direction { get; }
        Vector3 Look { get; }
        bool IsAccelerating { get; }
        event Action<bool> OnTest;
    }
}