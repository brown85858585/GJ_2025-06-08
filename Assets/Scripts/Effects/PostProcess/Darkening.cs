using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PostProcess
{
    public class Darkening : MonoBehaviour
    {
        [SerializeField] private float _fadeDuration = 5.0f;
        [Range(-10.0f, 0.0f)]
        [SerializeField] private float _minValue = -8.0f;
        [Range(0.0f, 10.0f)]
        [SerializeField] private float _maxValue = 0.0f;

        private ColorAdjustments _colorAdjustments;
        private DepthOfField _depthOfField;


        private Volume _volume;
        private Coroutine _fadeCoroutine;

        private void Awake()
        {
            _volume = FindObjectOfType<Volume>();
            _volume.profile.TryGet(out _colorAdjustments);
            _volume.profile.TryGet(out _depthOfField);
        }

        // Fade In
        public void FadeIn()
        {
            Debug.Log("FadeIn");

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(AnimateExposure(_minValue));
        }

        // Fade Out
        public void FadeOut()
        {
            Debug.Log("FadeOut");

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(AnimateExposure(_maxValue));
        }

        public void Blur()
        {
            _depthOfField.active = true;
        }

        public void Unblur()
        {
            _depthOfField.active = false;
        }

        private IEnumerator AnimateExposure(float targetValue)
        {
            var elapsedTime = 0f;
        
            while (elapsedTime < _fadeDuration)
            {
                _colorAdjustments.postExposure.value = Mathf.Lerp(_colorAdjustments.postExposure.value, targetValue, 
                    elapsedTime/_fadeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        private void OnDestroy()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }
        }
    }
}