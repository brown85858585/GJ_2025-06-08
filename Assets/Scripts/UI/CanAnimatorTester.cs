using UnityEngine;
using DG.Tweening;

public class CanAnimatorTester : MonoBehaviour
{
    [Header("Tilt Settings")]
    [SerializeField] private float tiltAngle = 30f;

    [SerializeField] private float[] patternDurations = {
        0.2f, 0.2f,   // раз-раз
        0.4f, 0.4f,   // –– ––
        0.2f, 0.2f,   // раз-раз
        0.4f          // ––
    };

    [SerializeField] private float intervalBetweenBeats = 0.1f;

    [Header("Water Particles")]
    [SerializeField] private ParticleSystem waterPS;

    private Sequence _beatSequence;
    private bool _isPouring = false;
    private Camera _camera;
    private float _startFov;

    private void Awake()
    {
        BuildBeatSequence();
        _beatSequence.Pause();
        _camera = Camera.main;
        _startFov = _camera.fieldOfView;
    }

    private void Update()
    {
        // Нажмите пробел, чтобы запустить/остановить анимацию
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePouring();
        }        
        // transform.position.z
    }

    private void BuildBeatSequence()
    {
        _beatSequence = DOTween.Sequence()
            .SetAutoKill(false)      // можем перезапускать
            .SetLoops(-1, LoopType.Restart);

        // Включаем частицы один раз
        _beatSequence.AppendCallback(() =>
        {
            if (waterPS && !waterPS.isPlaying)
                waterPS.Play();
        });

        // Собираем паттерн из наклонов
        foreach (float duration in patternDurations)
        {
            float half = duration * 0.5f;

            _beatSequence
                .Append(transform
                    .DOLocalRotate(new Vector3(0, 0, -tiltAngle), half)
                    .SetEase(Ease.OutQuad))
                .Append(transform
                    .DOLocalRotate(Vector3.zero, half)
                    .SetEase(Ease.InQuad))
                .AppendInterval(intervalBetweenBeats);
        }
    }

    /// <summary>
    /// Переключает полив: если не льётся – запускает, если льётся – останавливает.
    /// </summary>
    public void TogglePouring()
    {
        if (_isPouring)
            StopPouring();
        else
            StartPouring();
    }

    /// <summary>
    /// Запустить анимацию и частицы.
    /// </summary>
    public void StartPouring()
    {
        _isPouring = true;
        BuildBeatSequence();
        _beatSequence.Play();
    }

    /// <summary>
    /// Остановить анимацию, вернуть лейку в начальное положение и остановить частицы.
    /// </summary>
    public void StopPouring()
    {
        _isPouring = false;
        _beatSequence.Pause();
        if (waterPS && waterPS.isPlaying)
            waterPS.Stop();
        // плавно вернуть в 0°
        transform.DOLocalRotate(Vector3.zero, 0.2f).SetEase(Ease.OutQuad);
    }
}
