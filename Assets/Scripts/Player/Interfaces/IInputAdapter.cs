using UnityEngine;

namespace Player.Interfaces
{
    public interface IInputAdapter
    {
        Vector3 Direction { get; }
        Vector3 Look { get; }
        bool IsAccelerating { get; }
    }
}