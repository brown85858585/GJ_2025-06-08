using System;
using UnityEngine;

namespace Player
{
    public class PlayerRotateMovement : MonoBehaviour, IPlayerMovement
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Rotation Settings")]
        [SerializeField] private float rotationSpeed = 120f;
        
        private Rigidbody _rb;

        public void Move(Vector3 direction)
        {
            UpdRotete(direction);
            UpdMove(direction);
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

     
        private void UpdRotete(Vector3 direction)
        {
            float rotation = direction.x * rotationSpeed * Time.deltaTime;
            transform.Rotate(0, rotation, 0);
        }

        private void UpdMove(Vector3 direction)
        {
            Vector3 movement = transform.forward * direction.z;

            _rb.MovePosition(_rb.position + movement * moveSpeed * Time.deltaTime);
        }
    }
}