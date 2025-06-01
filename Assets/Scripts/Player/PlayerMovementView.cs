using Cinemachine;
using System;
using UnityEngine;

namespace Player
{
    public class PlayerMovementView : MonoBehaviour, IPlayerView, IPlayerMovement
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

        private void LateUpdate()
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

        public void Move(Vector3 direction)
        {
            UpdRotete(direction);
            FixedMove(direction);
        }

        public void UpdRotete(Vector3 direction)
        {
            float rotation = direction.x * rotationSpeed * Time.deltaTime;
            transform.Rotate(0, rotation, 0);
        }

        public void FixedMove(Vector3 direction)
        {
            Vector3 movement = transform.forward * direction.z;

            _rb.MovePosition(_rb.position + movement * moveSpeed * Time.deltaTime);
        }
    }
}
