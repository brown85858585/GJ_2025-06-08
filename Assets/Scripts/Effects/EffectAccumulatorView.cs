using Effects.PostProcess;
using PostProcess;
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
        
        public void FadeIn()
        {
            _darker.FadeIn();
        }
        public void FadeOut()
        {
            _darker.FadeOut();
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