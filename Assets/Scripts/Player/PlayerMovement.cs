using UnityEngine;

namespace Player
{
    public class PlayerMovement : MonoBehaviour, IPlayerMovement
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Rotation Settings")]
        [SerializeField] private float turnSmooth   = 5f;
        
        private Rigidbody _rb;
        private Vector3 _direction;
        private Transform _camTransform;
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
           
            CacheCamera();
        }

        private void LateUpdate()
        {
            if (_direction.sqrMagnitude > 0.001f)
            {
                RotateToForward();
            }
        }

        private void RotateToForward()
        {
            // a) Получаем нужный угол поворота (в градусах) с учётом мировой системы координат
            float targetAngle = Mathf.Atan2(_direction.x, _direction.z) * Mathf.Rad2Deg;
            // b) Плавно интерполируем текущий угол к целевому
            float smoothAngle = Mathf.LerpAngle(
                transform.eulerAngles.y, 
                targetAngle, 
                Time.deltaTime * turnSmooth
            );
            // c) Применяем итоговый поворот по оси Y
            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
        }

        public void Move(Vector3 direction)
        {
            Vector3 offsetDirection = direction.normalized;
            if (_camTransform)
            {
                // Базовые оси камеры
                Vector3 camForward = _camTransform.forward;
                Vector3 camRight = _camTransform.right;

                // Проецируем на плоскость, чтобы исключить наклон камеры
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                // Считаем направление движения в локальных осях камеры
                offsetDirection = (camForward * offsetDirection.z + camRight * offsetDirection.x).normalized;
            }else
            {
                CacheCamera();
            }

            _direction = offsetDirection;
            Vector3 movement = _direction * moveSpeed * Time.fixedDeltaTime;
            _rb.MovePosition(_rb.position + movement);
        }
        
        private void CacheCamera()
        {
            Camera cam = Camera.main; // найдёт объект с тегом "MainCamera"
            if (cam != null)
                _camTransform = cam.transform;
            else
                Debug.LogWarning("MainCamera не найдена! Проверьте тег.");
        }
    }
}