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

    [SerializeField] private int _maxCycle = 7;

    ApartmentWeatherEffects _apartmentWeatherEffects;
    ParkWeatherEffects _parkWeatherEffects;
    private IWeatherEffects[] _weatherEffectsArray;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);

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

    public void SetWeather(LocationType location, int cycle)
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

        ApplyWeatherForCurrentCycle();
    }

    public void SetLocation(LocationType location)
    {
        CurrentLocation = location;
        ApplyWeatherForCurrentCycle();
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

        ApplyWeatherForCurrentCycle();
    }

    public void NextCycle()
    {
        CurrentCycle++;

        if (CurrentCycle > _maxCycle)
        {
            CurrentCycle = 1;
        }

        Debug.Log("Current Cycle: " + CurrentCycle);

        ApplyWeatherForCurrentCycle();
    }

    private void ApplyWeatherForCurrentCycle()
    {
        WeatherType weatherType = GetWeatherTypeForCycle(CurrentCycle);

        if (_weatherEffectsArray[(int)CurrentLocation] != null)
        {
            _weatherEffectsArray[(int)CurrentLocation].ApplyWeather(weatherType);
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
            case 7: return WeatherType.ClearMorning;
            default: return WeatherType.ClearNoon;
        }
    }
}
