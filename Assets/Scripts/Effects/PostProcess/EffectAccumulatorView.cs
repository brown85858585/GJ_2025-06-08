using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Effects.PostProcess
{
    public class EffectAccumulatorView : MonoBehaviour
    {
        [SerializeField] private PostEffectsController postEffectsController;
        [SerializeField] private GameObject weather;

        private PostEffectsController _postEffectsController;
        private WeatherProvider _weather;
        private Vignette _vignette;
        private bool _isVignetteEnabled;

        private void Awake()
        {
            var weatherObj = Instantiate(weather);

            _postEffectsController = postEffectsController;
            _weather = weatherObj.GetComponent<WeatherProvider>();
        }

        public void Blur()
        {
            _postEffectsController.Blur();
        }

        public void Unblur()
        {
            _postEffectsController.Unblur();
        }

        public void FadeIn(float duration = -1, PostEffectProfile newProfile = PostEffectProfile.None)
        {
            if (newProfile != PostEffectProfile.None)
            {
                postEffectsController.FadeInWithSwitch(duration, newProfile);
            }
            else
            {
                postEffectsController.FadeIn(duration);
            }
        }

        public void FadeOut(float duration = -1, PostEffectProfile newProfile = PostEffectProfile.None)
        {
            if (newProfile != PostEffectProfile.None)
            {
                postEffectsController.FadeOutWithSwitch(duration, newProfile);
            }
            else
            {
                postEffectsController.FadeOut(duration);
            }
        }

        public void SetWeather(int cycle)
        {
            _weather.WeatherSystem.SetWeather(cycle);
        }

        public void DisableWeather()
        {
            _weather.WeatherSystem.DisableWeather();
        }
    }
}