using System;
using UnityEngine;

namespace Player
{
    public class PlayerMovementView : MonoBehaviour, IPlayerView
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        
        public Transform TransformPlayer { get;  set; }
        public event Action OnCollision;

        
        private Rigidbody _rb;
        private PlayerController _controller;
    
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
            Vector3 movement = delta * moveSpeed * Time.fixedDeltaTime;
            _rb.MovePosition(_rb.position + movement);
        }
    }
}
