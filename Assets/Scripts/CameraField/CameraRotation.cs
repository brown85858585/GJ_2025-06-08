using Player.Interfaces;
using UnityEngine;

namespace CameraField
{
    public class CameraRotation : MonoBehaviour
    {
        private  IInputAdapter _inputAdapter;
        private Transform _camera;


        [Header("Настройки")]  
        public float sensX = 2f;    
        public float sensY = 1f;     
        public float smoothTime = 0.1f;
        public float minYaw = 10f;
        public float maxYaw = 30f;
        public float minPitch = 20f;
        public float maxPitch = 40f;

        float targetYaw, targetPitch;
        float smoothYaw, smoothPitch;
        float velYaw, velPitch;

        public void Initialization(IInputAdapter inputAdapter, Transform cam)
        {
            _inputAdapter = inputAdapter;
            _camera = cam;
        }

        void Start()
        {
            Vector3 euler = _camera.rotation.eulerAngles;
            targetYaw = smoothYaw = euler.y;
            targetPitch = smoothPitch = euler.x;
        }

        void LateUpdate()
        {
            Vector3 lookInput = _inputAdapter.Look;

            // 1) обновляем целевые углы
            targetYaw   += lookInput.x * sensX * Time.deltaTime;
            targetYaw   = Mathf.Clamp(targetYaw, minYaw, maxYaw); // Ограничиваем Yaw 
            targetPitch -= lookInput.z * sensY * Time.deltaTime;
            targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

            // 2) сглаживаем к ним текущие
            smoothYaw   = Mathf.SmoothDampAngle(smoothYaw,   targetYaw,   ref velYaw,   smoothTime);
            smoothPitch = Mathf.SmoothDampAngle(smoothPitch, targetPitch, ref velPitch, smoothTime);

            // 3) применяем к повороту
            _camera.rotation = Quaternion.Euler(smoothPitch, smoothYaw, 0f);
        }
    }
}