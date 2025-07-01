using Effects.PostProcess;
using UnityEngine;

namespace Effects
{
    public class EffectAccumulatorView : MonoBehaviour
    {
        [SerializeField] private GameObject darker;
        [SerializeField] private GameObject weather;
        
        private Darkening _darker;
        private WeatherProvider _weather;

        private void Awake()
        {
            var darkerObj = Instantiate(darker);
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
            _darker.FadeIn(duration);
        }
        public void FadeOut(float duration = -1)
        {
            _darker.FadeOut(duration);
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