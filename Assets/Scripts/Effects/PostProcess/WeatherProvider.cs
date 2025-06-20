using UnityEngine;

namespace Effects.PostProcess
{
   public class WeatherProvider : MonoBehaviour
   {
      [SerializeField] private WeatherSystem _weatherSystem;

      public WeatherSystem WeatherSystem => _weatherSystem;
   }
}
