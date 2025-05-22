using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Darker : MonoBehaviour
{
    [SerializeField] private Volume _volume;
    [SerializeField] private float _fadeDuration = 5.0f;
    [SerializeField] private float _minValue = -8.0f;
    [SerializeField] private float _maxValue = 0.0f;

    private ColorAdjustments _colorAdjustments;

    private void Start()
    {
        //_volume.profile.TryGetSettings(out colorGrading);
        _volume.profile.TryGet(out _colorAdjustments);
    }

    // Fade In
    public void FadeIn()
    {
        StartCoroutine(AnimateExposure(_maxValue, _minValue));
    }

    // Fade Out
    public void FadeOut()
    {
        StartCoroutine(AnimateExposure(_minValue, _maxValue));
    }

    private IEnumerator AnimateExposure(float startValue, float targetValue)
    {
        float elapsedTime = 0f;

        while (elapsedTime < _fadeDuration)
        {
            _colorAdjustments.postExposure.value = Mathf.Lerp(startValue, targetValue, elapsedTime / _fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _colorAdjustments.postExposure.value = targetValue;
    }
}