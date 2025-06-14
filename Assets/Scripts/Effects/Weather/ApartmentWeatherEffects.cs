using System.Collections;
using UnityEngine;

public class ApartmentWeatherEffects : WeatherEffects
{
    public override LocationType Location => LocationType.Apartment;

    public override void ApplyWeather(WeatherType weatherType)
    {
        ResetAllEffects();

        switch (weatherType)
        {
            case WeatherType.ClearNoon:
                ApplyClearNoon();
                break;
            case WeatherType.FoggyMorning:
                ApplyFoggyMorning();
                break;
            case WeatherType.Cloudy:
                ApplyCloudy();
                break;
            case WeatherType.Overcast:
                ApplyOvercast();
                break;
            case WeatherType.Drizzle:
                ApplyDrizzle();
                break;
            case WeatherType.Rain:
                ApplyRain();
                break;
            case WeatherType.HeavyRain:
                ApplyHeavyRain();
                break;
            case WeatherType.Thunderstorm:
                ApplyThunderstorm();
                break;
            case WeatherType.ClearMorning:
                ApplyClearMorning();
                break;
        }
    }

    private void ApplyClearNoon()
    {
        Debug.Log("Apply Clear Noon");
    }

    private void ApplyFoggyMorning()
    {
        Debug.Log("Apply Foggy Morning");
    }

    private void ApplyCloudy()
    {
        Debug.Log("Apply Cloudy");
    }

    private void ApplyOvercast()
    {
        Debug.Log("Apply Overcast");
    }

    private void ApplyDrizzle()
    {
        Debug.Log("Apply Drizzle");
    }

    private void ApplyRain()
    {
        Debug.Log("Apply Rain");
    }

    private void ApplyHeavyRain()
    {
        Debug.Log("Apply HeavyRain");
    }

    private void ApplyThunderstorm()
    {
        Debug.Log("Apply Thunderstorm");
    }

    private void ApplyClearMorning()
    {
        Debug.Log("Apply Clear Morning");
    }
    private void ResetAllEffects()
    {
        
    }
}
