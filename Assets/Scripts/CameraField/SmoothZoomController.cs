using System.Collections;
using Cinemachine;
using Player.Interfaces;
using UnityEngine;

namespace CameraField
{
    public class SmoothZoomController : MonoBehaviour
    {
        [SerializeField] private float zoomStep = 5f;
        [SerializeField] private float minFOV = 20f;
        [SerializeField] private float maxFOV = 60f;
        [SerializeField] private float zoomSpeed = 10f;

        private float _targetFOV;
        private Coroutine _zoomCoroutine;
        private IInputAdapter _inputAdapter;
        private CinemachineVirtualCamera _vCam;

        public void Initialization(IInputAdapter inputAdapter, CinemachineVirtualCamera cam)
        {
            _inputAdapter = inputAdapter;
            _vCam = cam;
        }

        private void Start()
        {
            _targetFOV = _vCam.m_Lens.FieldOfView;

            _inputAdapter.OnZoomIn += () => ChangeZoom(-zoomStep);
            _inputAdapter.OnZoomOut += () => ChangeZoom(+zoomStep);
        }

        private void ChangeZoom(float delta)
        {
            // обновляем цель и ограничиваем её
            _targetFOV = Mathf.Clamp(_targetFOV + delta, minFOV, maxFOV);

            // если уже идёт корутина, перезапускаем её
            if (_zoomCoroutine != null)
                StopCoroutine(_zoomCoroutine);

            _zoomCoroutine = StartCoroutine(SmoothZoom());
        }

        private IEnumerator SmoothZoom()
        {
            // пока текущее FOV далеко от цели — движемся к ней
            while (!Mathf.Approximately(_vCam.m_Lens.FieldOfView, _targetFOV))
            {
                float current = _vCam.m_Lens.FieldOfView;
                float next = Mathf.MoveTowards(current, _targetFOV, zoomSpeed * Time.deltaTime);
                _vCam.m_Lens.FieldOfView = next;
                yield return null;
            }
            _zoomCoroutine = null;
        }
    }
}