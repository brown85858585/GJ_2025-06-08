using UnityEngine;

namespace UI
{ 
    [ExecuteAlways]
    public class FollowProjectionWithConstantSize : MonoBehaviour
    {
        public Transform target;
        public Vector3 worldOffset = Vector3.up * 1f;
        private Camera cam;
        public float scaleFactor = 1f; 
        
        public Vector3 _baseScale = Vector3.one;

        void Awake()
        {
            cam = Camera.main;
        }

        void LateUpdate()
        {
            if (target == null || cam == null) return;

            // восстанавливаем мировую позицию
            transform.position = target.position + worldOffset;

            transform.forward = cam.transform.forward;
            transform.localScale = _baseScale * scaleFactor;
        }
    }
}