using System.Collections;
using UnityEngine;

public abstract class WeatherEffects : MonoBehaviour
{
    public abstract LocationType Location { get; }

    private void OnEnable()
    {
        if (WeatherSystem.Instance != null)
        {
            WeatherSystem.Instance.RegisterEffects(this);
        }
        else
        {
            Debug.LogWarning("WeatherSystem instance is not available. Weather effects will not be registered.");
            StartCoroutine(RegisterWhenSystemIsReady());
        }
    }

    private void OnDisable()
    {
        if (WeatherSystem.Instance != null)
        {
            WeatherSystem.Instance.UnregisterEffects(this);
        }
    }

    public abstract void ApplyWeather(WeatherType weatherType);

    private IEnumerator RegisterWhenSystemIsReady()
    {
        while (WeatherSystem.Instance == null)
        {
            yield return null;
        }
        WeatherSystem.Instance.RegisterEffects(this);
    }
}
