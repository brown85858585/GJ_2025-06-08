public interface IWeatherEffects
{
    public LocationType Location { get; }
    public void ApplyWeather(WeatherType weatherType);
}
