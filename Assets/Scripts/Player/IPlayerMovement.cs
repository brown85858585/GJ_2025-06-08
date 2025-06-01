using UnityEngine;

namespace Player
{
    public interface IPlayerMovement
    {
        Transform CameraTransform { get; set; }
        void Move(Vector3 direction);
    }
}