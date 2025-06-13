using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherController : MonoBehaviour
{
    private WeatherEffects _weatherEffects;

    private void Start()
    {
        _weatherEffects = new ParkWeatherEffects();
        WeatherSystem.Instance.SetLocation(LocationType.Apartment);
        WeatherSystem.Instance.RegisterEffects(_weatherEffects);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            WeatherSystem.Instance.NextCycle();
        }
    }
}
