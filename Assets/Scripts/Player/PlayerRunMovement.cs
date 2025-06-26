using Player.Interfaces;
using UnityEngine;

namespace Player
{
    public class PlayerRunMovement : IPlayerMovement
    {
        private readonly IInputAdapter _input;
        private readonly float _rotationSpeed = 120f;

        private float _currentSpeed;
        
        private const float Acceleration = 40f;
        private const float SpeedMultiplier = 3;
        
        public float NormalizedSpeed { get;private set; }

        public PlayerRunMovement(IInputAdapter input)
        {
            _input = input;
        }
        public Vector3 Move(float moveSpeed, Transform playerTransform)
        {
            float targetSpeed = _input.Direction.z * moveSpeed * SpeedMultiplier;
            targetSpeed = Mathf.Clamp(targetSpeed, 0, float.MaxValue);
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, Acceleration * Time.fixedDeltaTime);
            
            Vector3 desiredAcceleration =  playerTransform.transform.forward * (_currentSpeed);

            // Мета для ускорения анимации 
            NormalizedSpeed = _currentSpeed /(moveSpeed * SpeedMultiplier);
            
            desiredAcceleration.y = 0;

            return desiredAcceleration;
        }
        
        public Quaternion Rotation(Transform transform, float rotationSpeed)
        {
            float yaw = _input.Direction.x * _rotationSpeed * Time.fixedDeltaTime;
            transform.Rotate(0f, yaw, 0f);
            return transform.rotation;
        }

        public void SpeedDrop(Rigidbody rb, Transform transform)
        {
            Debug.Log( "SpeedDrop called with current speed: " + _currentSpeed);
            _currentSpeed *= 0.2f;

            Vector3 reducedHorizontal = transform.forward * _currentSpeed;
            rb.velocity = new Vector3(
                reducedHorizontal.x,
                rb.velocity.y,
                reducedHorizontal.z
            );
        }
    }
}