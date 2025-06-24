using Player.Interfaces;
using UnityEngine;

namespace Player
{
    public class PlayerMovement : IPlayerMovement
    {
        private readonly IInputAdapter _input;
        
        private readonly PlayerModel _model;
        private Vector3 _direction;
        
        public Transform VirtualCamera;
        public float NormalizedSpeed { get; private set; }
        
        public PlayerMovement(IInputAdapter input, PlayerModel model)
        {
            _input = input;
            _model = model;
        }
        public Vector3 Move(float moveSpeed, Transform playerTransform)
        {
            var offsetDirection = _input.Direction.normalized;
            
            if (VirtualCamera)
            {
                // Базовые оси камеры
                var camForward = VirtualCamera.forward;
                var camRight = VirtualCamera.right;

                // Проецируем на плоскость, чтобы исключить наклон камеры
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                // Считаем направление движения в локальных осях камеры
                offsetDirection = (camForward * offsetDirection.z + camRight * offsetDirection.x).normalized;
            }
            else
            {
               Debug.LogWarning("Virtual camera is not set. Using input direction directly.");
            }

            _direction = offsetDirection;
            Vector3 movement = _direction * moveSpeed;

            return _model.Grounded ? movement : Vector3.zero;
        }

        public Quaternion Rotation(Transform transform, float rotationSpeed)
        {
            if (_input.Direction.sqrMagnitude > 0.001f)
            {
                // a) Получаем нужный угол поворота (в градусах) с учётом мировой системы координат
                float targetAngle = Mathf.Atan2(_direction.x, _direction.z) * Mathf.Rad2Deg;
                // b) Плавно интерполируем текущий угол к целевому
                float smoothAngle = Mathf.LerpAngle(
                    transform.eulerAngles.y,
                    targetAngle,
                    Time.deltaTime * rotationSpeed
                );
                // c) Применяем итоговый поворот по оси Y
                return Quaternion.Euler(0f, smoothAngle, 0f);
            }        
            return transform.rotation;
        }

        public void SpeedDrop(Rigidbody rb, Transform transform)
        {
        }
    }
}