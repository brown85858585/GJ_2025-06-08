using System.Collections;
using UnityEngine;

public class ParkWeatherEffects : MonoBehaviour, IWeatherEffects
{
    public LocationType Location => LocationType.Park;

    [Header("Lighting")]
    [SerializeField] private Light _directionalLight;

    [Header("Rain Effects")]
    [SerializeField] private ParticleSystem _heavyRainParticles;
    [SerializeField] private ParticleSystem _thunderRainParticles;

    [Header("Environment")]
    [SerializeField] private AudioSource _ambientAudioSource;

    [Header("Clear Noon")]
    [SerializeField] private Material _w1_SkyboxMaterial;
    [SerializeField] private float _w1_LightIntensity = 1.0f;
    [SerializeField] private Color _w1_LightColor = Color.white;

    [Header("FoggyMorning")]
    [SerializeField] private Material _w2_SkyboxMaterial;
    [SerializeField] private float _w2_fogDensity = 0.03f;
    [SerializeField] private float _w2_LightIntensity = 0.7f;
    [SerializeField] private Color _w2_LightColor = new Color(0.9f, 0.9f, 0.8f);

    [Header("Cloudy")]
    [SerializeField] private Material _w3_SkyboxMaterial;
    [SerializeField] private float _w3_LightIntensity;
    [SerializeField] private Color _w3_LightColor;
    
    [Header("Overcast")]
    [SerializeField] private Material _w4_SkyboxMaterial;
    [SerializeField] private float _w4_LightIntensity;
    [SerializeField] private Color _w4_LightColor;

    [Header("HeavyRain")]
    [SerializeField] private Material _w5_SkyboxMaterial;
    [SerializeField] private float _w5_LightIntensity;
    [SerializeField] private Color _w5_LightColor;
    [SerializeField] private ParticleSystem _HeavyRainParticles;

    [Header("Thunderstorm")]
    [SerializeField] private Material _w6_SkyboxMaterial;
    [SerializeField] private AudioClip _rainSound;
    [SerializeField] private AudioClip _thunderSound;
    [SerializeField] private float _w6_LightIntensity = 0.3f;
    [SerializeField] private Color _w6_LightColor;

    [Header("ClearMorning")]
    [SerializeField] private Material _w7_SkyboxMaterial;
    [SerializeField] private float _w7_LightIntensity = 1.0f;
    [SerializeField] private Color _w7_LightColor;

    private Coroutine _lightningRoutine = null;
    private Coroutine _lightningFlashRoutine = null;

    private void OnDestroy()
    {
        ResetAllEffects();
    }

    public void ApplyWeather(WeatherType weatherType)
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
        Debug.Log("Park: Apply Clear Noon");

        if (_w1_SkyboxMaterial != null)
        {
            RenderSettings.skybox = _w1_SkyboxMaterial;
        }

        if (_directionalLight != null)
        {
            _directionalLight.intensity = _w1_LightIntensity;
            _directionalLight.color = _w1_LightColor;
        }

        RenderSettings.fog = false;
    }

    private void ApplyFoggyMorning()
    {
        Debug.Log("Park: Apply Foggy Morning");

        if (_w2_SkyboxMaterial)
        {
            RenderSettings.skybox = _w2_SkyboxMaterial;
        }

        if (_directionalLight != null)
        {
            _directionalLight.intensity = _w2_LightIntensity;
            _directionalLight.color = _w2_LightColor;
        }

        RenderSettings.fog = true;
        RenderSettings.fogDensity = _w2_fogDensity;
    }

    private void ApplyCloudy()
    {
        Debug.Log("Park: Apply Cloudy");

        if (_w3_SkyboxMaterial)
        {
            RenderSettings.skybox = _w3_SkyboxMaterial;
        }

        if (_directionalLight != null)
        {
            _directionalLight.intensity = _w3_LightIntensity;
            _directionalLight.color = _w3_LightColor;
        }

        RenderSettings.fog = false;
    }

    private void ApplyOvercast()
    {
        Debug.Log("Park: Apply Overcast");

        if (_w4_SkyboxMaterial)
        {
            RenderSettings.skybox = _w4_SkyboxMaterial;
        }

        if (_directionalLight != null)
        {
            _directionalLight.intensity = _w4_LightIntensity;
            _directionalLight.color = _w4_LightColor;
        }

        RenderSettings.fog = false;
    }

    private void ApplyHeavyRain()
    {
        Debug.Log("Park: Apply HeavyRain");

        if (_w5_SkyboxMaterial)
        {
            RenderSettings.skybox = _w5_SkyboxMaterial;
        }

        if (_directionalLight != null)
        {
            _directionalLight.intensity = _w5_LightIntensity;
            _directionalLight.color = _w5_LightColor;
        }

        RenderSettings.fog = false;
    }

    private void ApplyThunderstorm()
    {
        Debug.Log("Park: Apply Thunderstorm");

        if (_w6_SkyboxMaterial)
        {
            RenderSettings.skybox = _w6_SkyboxMaterial;
        }

        if (_directionalLight != null)
        {
            _directionalLight.intensity = _w6_LightIntensity;
            _directionalLight.color = _w6_LightColor;
        }

        RenderSettings.fog = false;

        if (_thunderRainParticles != null)
        {
            _thunderRainParticles.Play();
        }

        if (_ambientAudioSource != null && _rainSound != null)
        {
            _ambientAudioSource.clip = _rainSound;
            _ambientAudioSource.Play();
        }

        if (_lightningRoutine != null)
        {
            StopCoroutine(_lightningRoutine);
        }

        _lightningRoutine = StartCoroutine(ThunderRoutine());
    }

    private void ApplyClearMorning()
    {
        Debug.Log("Park: Apply Clear Morning");

        if (_w7_SkyboxMaterial)
        {
            RenderSettings.skybox = _w7_SkyboxMaterial;
        }

        RenderSettings.fog = false;
    }

    private IEnumerator ThunderRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(5f, 15f));
            PlayThunder();
        }
    }

    private void PlayThunder()
    {
        if (_lightningFlashRoutine != null)
        {
            StopCoroutine(_lightningFlashRoutine);
        }

        if (_directionalLight != null)
        {
            _lightningFlashRoutine = StartCoroutine(LightningFlashRoutine());
        }

        if (_ambientAudioSource != null)
        {
            _ambientAudioSource.PlayOneShot(_thunderSound);
        }
    }

    private IEnumerator LightningFlashRoutine()
    {
        _directionalLight.intensity = 1.5f;
        yield return new WaitForSeconds(0.1f);
        _directionalLight.intensity = 0.3f;
    }

    private void ResetAllEffects()
    {
        if (_lightningRoutine != null)
        {
            StopCoroutine(_lightningRoutine);
        }

        if (_lightningFlashRoutine != null)
        {
            StopCoroutine(_lightningFlashRoutine);
        }

        RenderSettings.fog = false;

        if (_directionalLight != null)
        {
            _directionalLight.intensity = 1.0f;
        }

        if (_heavyRainParticles)
        {
            _heavyRainParticles.Stop();
        }

        if (_thunderRainParticles != null)
        {
            _thunderRainParticles.Stop();
        }

        if (_ambientAudioSource != null)
        {
            _ambientAudioSource.Stop();
        }
    }
}
