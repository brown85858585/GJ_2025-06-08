using System;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class PlayerView : MonoBehaviour
    {
        private static readonly int Move = Animator.StringToHash("Move");
        private static readonly float Verticale = Animator.StringToHash("Verticale");
        private static readonly float Horizontal = Animator.StringToHash("Horizontal");
        private static readonly int IsDancing = Animator.StringToHash("IsDancing");

        [SerializeField] private Animator animator;
        [Range(0f,3f)]
        [SerializeField] private float animatorOffset = 1f;
        [SerializeField] private Transform rightHand;
        
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
        private Transform _saveLastParentObj;

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

        public void SetWalkAnimation(Vector3 walking)
        {
            //var speed = walking / (MoveSpeed * animatorOffset);
            animator.SetFloat("Verticale", walking.z);
            animator.SetFloat("Horizontal",walking.x);
            //if (speed > 0.1f)
            //{
           //     animator.SetBool(IsDancing, false);
           // }
        }

        public void StartDanceAnimation()
        {
            animator.SetBool(IsDancing, true);
        }

        public void TakeObject(Transform obj)
        {
            _saveCurrentObj = obj;

            var rb = _saveCurrentObj.gameObject.GetComponent<Rigidbody>();
            Destroy(rb);       
            
            _saveLastParentObj = obj.parent;
            obj.SetParent(rightHand.transform);
            obj.position = rightHand.position;
        }

        public void PutTheItemDown()
        {
            if (_saveCurrentObj == null) return;
            
            _saveCurrentObj.gameObject.AddComponent<Rigidbody>().mass = 0.2f;

            _saveCurrentObj.SetParent(_saveLastParentObj);
            _saveCurrentObj.transform.rotation = Quaternion.Euler(Vector3.zero);
            _saveLastParentObj = null;
            _saveCurrentObj = null;
        }
    }
}