using Cinemachine;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player
{
    public class PlayerView : MonoBehaviour, IPlayerView
    {
        [SerializeField] private PlayerRotateMovement playerRotateMovement;
        [SerializeField] private PlayerMovement playerMovement;
        public IPlayerMovement Movement { get; set; }
        public Transform TransformPlayer { get;  set; }
        public event Action OnCollision;
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

        private PlayerController _controller;
        
        private void LateUpdate()
        {
            _controller.Update();
        }

        public void Initialize(PlayerController controller)
        {
            _controller = controller;
        }
        
        private void Awake()
        {
            Movement = GetComponent<IPlayerMovement>();
            TransformPlayer = transform;
        }
        
        private void OnCollisionEnter(Collision other)
        {
            OnCollision?.Invoke();
        }
    }
}
