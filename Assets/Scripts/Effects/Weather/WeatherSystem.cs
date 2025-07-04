using UnityEngine;

public enum LocationType { Park, Apartment }

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
    ThunderstormWithLightning,
    ClearMorning,
    Default
}

public class WeatherSystem : MonoBehaviour
{
    public static WeatherSystem Instance { get; private set; }
    public LocationType CurrentLocation { get; private set; }
    public int CurrentCycle { get; private set; } = 1;

    [SerializeField] private int _maxCycle = 8;

    ApartmentWeatherEffects _apartmentWeatherEffects;
    ParkWeatherEffects _parkWeatherEffects;
    private IWeatherEffects[] _weatherEffectsArray;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            _apartmentWeatherEffects = FindObjectOfType<ApartmentWeatherEffects>();
            _parkWeatherEffects = FindObjectOfType<ParkWeatherEffects>();

            _weatherEffectsArray = new IWeatherEffects[2];
            _weatherEffectsArray[(int)LocationType.Apartment] = _apartmentWeatherEffects;
            _weatherEffectsArray[(int)LocationType.Park] = _parkWeatherEffects;

            CurrentLocation = LocationType.Apartment;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ButtonSetWeather()
    {
        CurrentCycle = Random.Range(1, 9);
        SetWeather(CurrentCycle);
    }

    public void ButtonDisableWeather()
    {
        DisableWeather();
    }

    public void SetWeather(int cycle, LocationType location = LocationType.Park, Transform transformParam = null)
    {
        CurrentLocation = location;

        if (cycle < 1)
        {
            cycle = 1;
        }
        else if (cycle > _maxCycle)
        {
            cycle = _maxCycle;
        }

        CurrentCycle = cycle;

        ApplyWeatherForCurrentCycle(cycle, location, transformParam);
    }

    public void DisableWeather()
    {
        ApplyWeather(WeatherType.Default);
    }

    public void SetLocation(LocationType location)
    {
        _weatherEffectsArray[(int)CurrentLocation].ResetAllEffects();
        CurrentLocation = location;
        ApplyWeatherForCurrentCycle(CurrentCycle, CurrentLocation);
    }

    public void SetSycle(int cycle)
    {
        if (cycle < 1)
        {
            cycle = 1;
        }
        else if (cycle > _maxCycle)
        {
            cycle = _maxCycle;
        }

        CurrentCycle = cycle;

        Debug.Log("Current Cycle: " + CurrentCycle);

        ApplyWeatherForCurrentCycle(CurrentCycle, CurrentLocation);
    }

    public void NextCycle()
    {
        CurrentCycle++;

        if (CurrentCycle > _maxCycle)
        {
            CurrentCycle = 1;
        }

        Debug.Log("Current Cycle: " + CurrentCycle);

        ApplyWeatherForCurrentCycle(CurrentCycle, CurrentLocation);
    }

    private void ApplyWeatherForCurrentCycle(int cycle, LocationType location = LocationType.Park, Transform transformParam = null)
    {
        WeatherType weatherType = GetWeatherTypeForCycle(cycle);
        ApplyWeather(weatherType, location, transformParam);
    }

    private void ApplyWeather(WeatherType weatherType, LocationType location = LocationType.Park, Transform transformParam = null)
    {
        if (_weatherEffectsArray[(int)location] != null)
        {
            _weatherEffectsArray[(int)location].ApplyWeather(weatherType, transformParam);
        }
        else
        {
            Debug.Log("WeatherSystem: _currentEffects is null!");
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
            case 5: return WeatherType.HeavyRain;
            case 6: return WeatherType.Thunderstorm;
            case 7: return WeatherType.ThunderstormWithLightning;
            case 8: return WeatherType.ClearMorning;
            default: return WeatherType.Default;
        }
    }
}
