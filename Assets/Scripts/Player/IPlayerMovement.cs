using UnityEngine;

namespace Player
{
    public interface IPlayerMovement
    {
        Vector3 Move(float moveSpeed, float staminaDecrease);
        Quaternion Rotation(Transform transform, float rotationSpeed);
        void SpeedDrop(Rigidbody rb, Transform transform);
        public float NormalizedSpeed { get; }
    }
}