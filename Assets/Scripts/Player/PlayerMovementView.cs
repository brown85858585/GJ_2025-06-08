using Cinemachine;
using System;
using UnityEngine;

namespace Player
{
    public class PlayerMovementView : MonoBehaviour, IPlayerView
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Rotation Settings")]
        [SerializeField] private float rotationSpeed = 120f;

        public Transform TransformPlayer { get;  set; }
        public event Action OnCollision;

        private CinemachineVirtualCamera virtualCamera;
        private CinemachineTransposer transposer;


        private Rigidbody _rb;
        private PlayerController _controller;

        private void Start()
        {

        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            TransformPlayer = transform;
        }

        private void FixedUpdate()
        {
            _controller.Update();
        }
        public void Initialize(PlayerController controller)
        {
            _controller = controller;
        }

        private void OnCollisionEnter(Collision other)
        {
            OnCollision?.Invoke();
        }

        public void Move(Vector3 delta)
        {
            UpdRotete();
            FixedMove();

        }

        void UpdRotete()
        {
            // Поворот делаем в Update
            float horizontal = Input.GetAxis("Horizontal");
            float rotation = horizontal * rotationSpeed * Time.deltaTime;
            transform.Rotate(0, rotation, 0);
        }

        void FixedMove()
        {
            // Движение в FixedUpdate
            float vertical = Input.GetAxis("Vertical");
            Vector3 movement = transform.forward * vertical;

            _rb.MovePosition(_rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }
}
