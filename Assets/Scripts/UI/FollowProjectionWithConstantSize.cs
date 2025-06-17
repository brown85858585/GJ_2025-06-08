using UnityEngine;

namespace UI
{ 
    [ExecuteAlways]
    public class FollowProjectionWithConstantSize : MonoBehaviour
    {
        public Transform target;
        public Vector3 worldOffset = Vector3.up * 1f;
        private Camera cam; 

        void Awake()
        {
            cam = Camera.main;
        }

        void LateUpdate()
        {
            if (target == null || cam == null) return;

            transform.position = target.position + worldOffset;

            transform.forward = cam.transform.forward;
        }
    }
}