using UnityEngine;

namespace Player.Interfaces
{
    public interface IInputAdapter
    {
        Vector3 Direction { get; }
        bool IsAccelerating { get; }
    }
}