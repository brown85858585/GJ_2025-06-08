using System;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class PlayerView : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField]
        private float moveSpeed = 5f;
        [SerializeField]
        private Button moveForwardButton;
        [SerializeField]
        private float groundDrag = 7f;
        
        [Header("Rotation Settings")]
        [SerializeField]
        private float turnSmooth = 5f;
        [SerializeField]
        private LayerMask whatIsGround;
        
        public float MoveSpeed => moveSpeed;
        public float GroundDrag => groundDrag;
        public float TurnSmooth => turnSmooth;
        public LayerMask WhatIsGround => whatIsGround;

        public Rigidbody Rigidbody { get; private set; }
        public CapsuleCollider CapsuleCollider { get; private set; }
        
        public event Action OnCollision;
        public event Action OnUpdate;
        public event Action OnButtonClick;

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            CapsuleCollider = GetComponent<CapsuleCollider>();
            
            moveForwardButton.onClick.AddListener(() => OnButtonClick?.Invoke());
        }

        private void FixedUpdate()
        {
            OnUpdate?.Invoke();
        }

        private void OnCollisionEnter(Collision other)
        {
            OnCollision?.Invoke();
        }

        private void OnDestroy()
        { 
            moveForwardButton.onClick.RemoveAllListeners();
        }
    }
}