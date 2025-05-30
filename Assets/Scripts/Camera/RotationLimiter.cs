using Cinemachine;
using UnityEngine;

namespace Camera
{
    [ExecuteAlways]
    [SaveDuringPlay]
    [AddComponentMenu("Cinemachine/Extensions/RotationLimiter")]
    public class RotationLimiter : CinemachineExtension
    {
        [Header("Yaw (горизонталь)")]
        public float minYaw = -30f;
        public float maxYaw =  30f;

        [Header("Pitch (вертикаль)")]
        public float minPitch = -10f;
        public float maxPitch =  50f;

        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage,
            ref CameraState state,
            float deltaTime)
        {
            // Ограничиваем только после того, как Cinemachine рассчитала Aim
            if (stage == CinemachineCore.Stage.Aim)
            {
                Vector3 angles = state.RawOrientation.eulerAngles;
                angles.x = NormalizeAngle(angles.x);
                angles.y = NormalizeAngle(angles.y);

                angles.x = Mathf.Clamp(angles.x, minPitch, maxPitch);
                angles.y = Mathf.Clamp(angles.y, minYaw,   maxYaw);

                state.RawOrientation = Quaternion.Euler(angles);
            }
        }

        static float NormalizeAngle(float a)
        {
            // Переводим [0–360] → [–180–+180]
            if (a > 180f) a -= 360f;
            return a;
        }
    }
}