using UnityEngine;

public interface IWeatherEffects
{
    public LocationType Location { get; }
    public void ApplyWeather(WeatherType weatherType, Transform transformParam = null);
    public void ResetAllEffects();
}
