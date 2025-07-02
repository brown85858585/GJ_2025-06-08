using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Effects.PostProcess
{
    public class Darkening : MonoBehaviour
    {
        [Header("Default settings (используются, если в метод не передана своя длительность)")]
        [SerializeField] private float _defaultFadeDuration = 1.0f;

        [Header("Экспозиция")]
        [Range(-15.0f, 0.0f)]
        [SerializeField] private float _minValue = -15.0f;
        [Range(0.0f, 10.0f)]
        [SerializeField] private float _maxValue = 0.0f;

        private ColorAdjustments _colorAdjustments;
        private DepthOfField _depthOfField;
        private Volume _volume;
        private Coroutine _fadeCoroutine;

        public void SetVolume (Volume volume)
        {
            _volume = volume;

            if (_volume != null)
            {
                _volume.profile.TryGet(out _colorAdjustments);
                _volume.profile.TryGet(out _depthOfField);
            }
        }
        
        /* ------------------------------------------------------------------ */
        /*                              PUBLIC API                            */
        /* ------------------------------------------------------------------ */

        /// <summary>Фейд-ин (ярко). Если duration &lt;= 0 — мгновенно.</summary>
        public void FadeIn(float duration = -1f, Action onOnFadeInComplete = null)
        {
            StartFade(_maxValue, duration, onOnFadeInComplete);
        }

        /// <summary>Фейд-аут (темно). Если duration &lt;= 0 — мгновенно.</summary>
        public void FadeOut(float duration = -1f, Action onOnFadeOutComplete = null)
        {
            StartFade(_minValue, duration, onOnFadeOutComplete);
        }

        public void Blur()    => _depthOfField.active = true;
        public void Unblur()  => _depthOfField.active = false;

        /* ------------------------------------------------------------------ */
        /*                             INTERNALS                              */
        /* ------------------------------------------------------------------ */

        private void StartFade(float targetValue, float duration, Action onOnFadeComplete)
        {
            // если пользователь не передал длительность, берём дефолт
            if (duration < 0f) duration = _defaultFadeDuration;

            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);

            _fadeCoroutine = StartCoroutine(AnimateExposure(targetValue, duration, onOnFadeComplete));
        }

        private IEnumerator AnimateExposure(float targetValue, float duration, Action onOnFadeComplete)
        {
            // мгновенный переходw
            if (duration <= 0f)
            {
                _colorAdjustments.postExposure.value = targetValue;
                yield break;
            }

            float startValue = _colorAdjustments.postExposure.value;
            float time       = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;              // 0 → 1
                _colorAdjustments.postExposure.value =
                    Mathf.Lerp(startValue, targetValue, t);
                yield return null;
            }

            onOnFadeComplete?.Invoke();
            _colorAdjustments.postExposure.value = targetValue;
        }

        private void OnDestroy()
        {
            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);
        }
    }
}
