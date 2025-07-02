using System;
using Effects.PostProcess;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Effects
{
    public class EffectAccumulatorView : MonoBehaviour
    {
        [SerializeField] private Volume globalVolume;
        [SerializeField] private GameObject darker;
        [SerializeField] private GameObject weather;

        private Volume _globalVolume;
        private Darkening _darker;
        private WeatherProvider _weather;
        private Vignette _vignette;
        private bool _isVignetteEnabled;
        
        public event Action OnFadeInComplete;
        public event Action OnFadeOutComplete;

        private void Awake()
        {
            _globalVolume = Instantiate(globalVolume);

            var darkerObj = Instantiate(darker);
            var componentDarker = darkerObj.GetComponent<Darkening>();
            componentDarker.SetVolume(_globalVolume);
            var weatherObj = Instantiate(weather);

            _darker = darkerObj.GetComponent<Darkening>();
            _weather = weatherObj.GetComponent<WeatherProvider>();
        }

        public void Blur()
        {
            _darker.Blur();
        }

        public void Unblur()
        {
            _darker.Unblur();
        }

        public void FadeIn(float duration = -1)
        {
            _darker.FadeIn(OnFadeInComplete,duration);
            
        }

        public void FadeOut(float duration = -1)
        {
            _darker.FadeOut(OnFadeOutComplete, duration);
        }

        public void SetWeather(int cycle)
        {
            _weather.WeatherSystem.SetWeather(cycle);
        }

        public void DisableWeather()
        {
            _weather.WeatherSystem.DisableWeather();
        }

        public void VignetteToggle()
        {
            _globalVolume.profile.TryGet(out _vignette);
            
            if (_vignette == null) return;
            _isVignetteEnabled = !_isVignetteEnabled;
            _vignette.active = _isVignetteEnabled;
        }
    }
}