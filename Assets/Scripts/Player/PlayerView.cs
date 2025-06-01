using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Player
{
    public class PlayerView : MonoBehaviour, IPlayerView
    {
        public Transform TransformPlayer { get;  set; }
        public event Action OnCollision;
        
        public IPlayerMovement Movement { get; set; }
        
        private PlayerController _controller;
        
        private void Awake()
        {
            Movement = GetComponent<IPlayerMovement>();
            TransformPlayer = transform;
        }
        
        private void FixedUpdate()
        {
            _controller.FixedUpdate();
        }

        public void SetNormalMovement()
        {
            Destroy(Movement  as Component);
            Movement = transform.AddComponent<PlayerMovement>();
        }

        public void SetRotateMovement()
        {
            Destroy(Movement  as Component);
            Movement = transform.AddComponent<PlayerRotateMovement>();
        }
        
        public void Initialize(PlayerController controller)
        {
            _controller = controller;
        }
        
        private void OnCollisionEnter(Collision other)
        {
            OnCollision?.Invoke();
        }
    }
}
