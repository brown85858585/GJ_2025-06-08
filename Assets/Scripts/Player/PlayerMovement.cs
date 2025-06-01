using System;
using UnityEngine;

namespace Player
{
    public class PlayerMovement : MonoBehaviour, IPlayerMovement
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Rotation Settings")]
        [SerializeField] private float turnSmooth   = 10f;
        
        private Rigidbody _rb;
        private Vector3 _direction;
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
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
            _direction = direction.normalized;
            Vector3 movement = direction * moveSpeed * Time.fixedDeltaTime;
            _rb.MovePosition(_rb.position + movement);
        }
    }
}