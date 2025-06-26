using System;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class PlayerView : MonoBehaviour
    {
        private static readonly int IsMove = Animator.StringToHash("Move");
        private static readonly int IsRun = Animator.StringToHash("Run");
        private static readonly int PositionX = Animator.StringToHash("PositionX");
        private static readonly int PositionY = Animator.StringToHash("PositionY");

        [SerializeField] private Animator animator;
        [SerializeField] private Transform rightHand;
        [SerializeField] private PlayerDialogueView dialogueView;
        
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

        private Transform _saveCurrentObj;
        private bool _moveSavedObject;

        private const float dampTime = 0.1f;

        private static readonly Quaternion _rotationOffset = Quaternion.Euler(180f, 0f, 0f);
        public float MoveSpeed => moveSpeed;
        public float GroundDrag => groundDrag;
        public float TurnSmooth => turnSmooth;
        public LayerMask WhatIsGround => whatIsGround;
        public PlayerDialogueView DialogueView => dialogueView;
        
        public Rigidbody Rigidbody { get; private set; }
        public CapsuleCollider CapsuleCollider { get; private set; }
        
        public event Action<Collision> OnCollision;
        public event Action OnUpdate;

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            CapsuleCollider = GetComponent<CapsuleCollider>();
        }
        private void FixedUpdate()
        {
            OnUpdate?.Invoke();
            
            if (_moveSavedObject)
            {
                _saveCurrentObj.position = rightHand.position;
                _saveCurrentObj.rotation = rightHand.rotation * _rotationOffset;
                
            }
        }

        private void OnCollisionEnter(Collision Collision)
        {
            OnCollision?.Invoke(Collision);
        }

        public void SetRunAnimation(float speed)
        {
            animator.SetBool(IsRun, true);
            animator.SetFloat(IsMove, speed);
        }
        
        public void SetWalkAnimation(Vector3 input)
        {
            animator.SetFloat(IsMove, 0);
            animator.SetBool(IsRun, false);
            // ВАРИАНТ A: направление движения относительно камеры
            // Vector3 camFwd = Camera.main.transform.forward; camFwd.y = 0f;
            // Vector3 camRight = Camera.main.transform.right; camRight.y = 0f;
            // worldMove = input.z * camFwd.normalized + input.x * camRight.normalized;
            // ВАРИАНТ B: реальная скорость Rigidbody (комментируем вариант A, если нужен этот)
            Vector3 worldMove;
            Vector3 vel = Rigidbody.velocity;
            worldMove = new Vector3(vel.x, 0, vel.z);
            
            /* 2. Конвертируем в локальные координаты */
            Vector3 localMove = transform.InverseTransformDirection(worldMove);

            /* 3. Нормализуем, оставляем диапазон -1…1  */
            Vector2 planar = new Vector2(localMove.x, localMove.z);
            if (planar.sqrMagnitude > 1f)
                planar.Normalize();                   // чтобы не «вываливаться» за края BlendTree
            
            animator.SetFloat(PositionX, planar.x, dampTime, Time.deltaTime);
            animator.SetFloat(PositionY, planar.y, dampTime, Time.deltaTime);
        }

        public void TakeObject(Transform obj)
        {
            _saveCurrentObj = obj;

            var rb = _saveCurrentObj.gameObject.GetComponent<Rigidbody>();
            rb.useGravity = false;
            _saveCurrentObj.gameObject.GetComponent<BoxCollider>().enabled = false;
            
            _moveSavedObject = true;
            obj.position = rightHand.position;
        }

        public void PutTheItemDown()
        {
            if (_saveCurrentObj == null) return;
            
            var rb = _saveCurrentObj.gameObject.GetComponent<Rigidbody>();
            rb.useGravity = true;
            _saveCurrentObj.gameObject.GetComponent<BoxCollider>().enabled = true;
  
            
            _moveSavedObject = false;_saveCurrentObj.transform.rotation = Quaternion.Euler(Vector3.zero);
            
            _saveCurrentObj = null;
        }
    }
}