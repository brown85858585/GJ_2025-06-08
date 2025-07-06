using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Effects.PostProcess
{
    public class PostEffectsController : MonoBehaviour
    {
        public Volume volume;
        public VolumeProfileSelector profileSelector;
        public float fadeDuration = 1f;

        private ColorAdjustments _colorAdj;
        private DepthOfField _depthOfField;
        private bool _isFading;

        void Awake()
        {
            // Получаем ColorAdjustments
            if (!volume.profile.TryGet(out _colorAdj))
                Debug.LogError("No ColorAdjustments in current profile!");

            // Получаем DepthOfField (если есть)
            if (!volume.profile.TryGet(out _depthOfField))
                Debug.LogWarning("No DepthOfField in current profile! Blur methods will do nothing.");
        }

        // Переключение профиля с фейдом
        public void SwitchWithFade(PostEffectProfile newProfile, float duration)
        {
            if (_isFading) return;
            StartCoroutine(FadeAndSwitch(newProfile, duration));
        }
        public void SwitchWithFade(PostEffectProfile newProfile)
            => SwitchWithFade(newProfile, fadeDuration);

        // Fade Out
        public void FadeOut(float duration)
        {
            if (_isFading) return;
            StartCoroutine(FadeOnly(0f, -15f, duration));
        }
        public void FadeOut() => FadeOut(fadeDuration);

        // Fade In
        public void FadeIn(float duration)
        {
            if (_isFading) return;
            StartCoroutine(FadeOnly(-15f, 0f, duration));
        }
        public void FadeIn() => FadeIn(fadeDuration);

        // Включить размытие
        public void Blur()
        {
            if (_depthOfField != null)
                _depthOfField.active = true;
        }

        // Отключить размытие
        public void Unblur()
        {
            if (_depthOfField != null)
                _depthOfField.active = false;
        }

        private IEnumerator FadeOnly(float from, float to, float duration)
        {
            _isFading = true;
            yield return Fade(from, to, duration);
            _isFading = false;
        }

        private IEnumerator FadeAndSwitch(PostEffectProfile newProfile, float duration)
        {
            _isFading = true;

            // Fade Out
            yield return Fade(0f, -15f, duration);

            // Switch profile
            profileSelector.SetProfile(newProfile);
            volume.profile.TryGet(out _colorAdj);
            volume.profile.TryGet(out _depthOfField);

            // Fade In
            yield return Fade(-15f, 0f, duration);

            _isFading = false;
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            if (duration < 0)
                duration = fadeDuration;

            float time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                _colorAdj.postExposure.value = Mathf.Lerp(from, to, t);
                yield return null;
            }

            _colorAdj.postExposure.value = to;
        }
    }
}
