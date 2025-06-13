using UnityEngine;

public enum LocationType { Apartment, Park }

public enum WeatherType
{
    ClearNoon,
    FoggyMorning,
    Cloudy,
    Overcast,
    Drizzle,
    Rain,
    HeavyRain,
    Thunderstorm,
    ClearMorning
}

public class WeatherSystem : MonoBehaviour
{
    public static WeatherSystem Instance { get; private set; }

    public LocationType CurrentLocation { get; private set; }
    public int CurrentCycle { get; private set; } = 1;

    private WeatherEffects _currentEffects;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetLocation(LocationType location)
    {
        CurrentLocation = location;
        ApplyWeatherForCurrentCycle();
    }

    public void NextCycle()
    {
        CurrentCycle++;

        if (CurrentCycle > 11)
        {
            CurrentCycle = 1;
        }

        Debug.Log("Current Cycle: " + CurrentCycle);

        ApplyWeatherForCurrentCycle();
    }

    private void ApplyWeatherForCurrentCycle()
    {
        // Определяем тип погоды для текущего цикла
        WeatherType weatherType = GetWeatherTypeForCycle(CurrentCycle);

        // Применяем эффекты для текущей локации
        if (_currentEffects != null)
        {
            _currentEffects.ApplyWeather(weatherType);
        }
    }

    private WeatherType GetWeatherTypeForCycle(int cycle)
    {
        switch (cycle)
        {
            case 1: return WeatherType.ClearNoon;
            case 2: return WeatherType.FoggyMorning;
            case 3: return WeatherType.Cloudy;
            case 4: return WeatherType.Overcast;
            case 5: return WeatherType.Drizzle;
            case 6: return WeatherType.Rain;
            case 7: return WeatherType.HeavyRain;
            case 8:
            case 9:
            case 10: return WeatherType.Thunderstorm;
            case 11: return WeatherType.ClearMorning;
            default: return WeatherType.ClearNoon;
        }
    }

    public void RegisterEffects(WeatherEffects effects)
    {
        if (effects.Location == CurrentLocation)
        {
            _currentEffects = effects;
            ApplyWeatherForCurrentCycle();
        }
    }

    public void UnregisterEffects(WeatherEffects effects)
    {
        if (_currentEffects == effects)
        {
            _currentEffects = null;
        }
    }
}
