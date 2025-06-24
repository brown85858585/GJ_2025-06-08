using System;
using UnityEngine;

namespace CameraField
{
   public class CameraDependence : MonoBehaviour
   {
      [SerializeField] private Camera _camera;
      [SerializeField] private SmoothZoomController _smoothZoom;
   
      public Camera Cam => _camera;

      private void Awake()
      {
         _smoothZoom.OnTargetFOVChanged += UpdateCameraProjection;
      }

      private void UpdateCameraProjection(float obj)
      {
         if (_camera == null) return;

         // Обновляем проекцию камеры в зависимости от FOV
         _camera.fieldOfView = obj;
      }
   }
}
