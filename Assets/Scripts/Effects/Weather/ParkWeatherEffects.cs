using System.Collections;
using TMPro;
using UnityEngine;

public class ParkWeatherEffects : MonoBehaviour, IWeatherEffects
{
    public LocationType Location => LocationType.Park;

    [Header("Lighting")]
    [SerializeField] private Light _directionalLight;
    [SerializeField] private Light _sunForWindow;
    [SerializeField] private float _sunForWindowMultiplyer = 100.0f;
    [SerializeField] private Light _lightning;
    [SerializeField] private float _lightningMinInterval = 2.0f;
    [SerializeField] private float _lightningMaxInterval = 3.0f;
    [SerializeField] private float _lightningFlashInterval_1 = 0.1f;
    [SerializeField] private float _lightningFlashInterval_2 = 0.05f;
    [SerializeField] private float _lightningFlashInterval_3 = 0.05f;
    [SerializeField] private float _lightningSkyboxExposure_1 = 2.0f;
    [SerializeField] private float _lightningSkyboxExposure_2 = 1.0f;
    [SerializeField] private float _lightningSkyboxExposure_3 = 2.0f;
    [SerializeField] private float _lightningSkyboxExposure_4 = 1.0f;

    [Header("Environment")]
    [SerializeField] private AudioSource _ambientAudioSource;
    [SerializeField] private Material _defaultSkyBoxMaterial;

    [Header("Room Window")]
    [SerializeField] private Material _roomWindowMaterial;

    [Header("Clear Noon")]
    [SerializeField] private TextMeshProUGUI _txtClearNoon;
    [SerializeField] private Material _w1_SkyboxMaterial;
    [SerializeField] private float _w1_LightIntensity = 1.0f;
    [SerializeField] private Color _w1_LightColor = Color.white;

    [Header("Foggy Morning")]
    [SerializeField] private TextMeshProUGUI _txtFoggyMorning;
    [SerializeField] private Material _w2_SkyboxMaterial;
    [SerializeField] private float _w2_fogDensity = 0.03f;
    [SerializeField] private float _w2_LightIntensity = 0.7f;
    [SerializeField] private Color _w2_LightColor = new Color(0.9f, 0.9f, 0.9f);

    [Header("Cloudy")]
    [SerializeField] private TextMeshProUGUI _txtCloudy;
    [SerializeField] private Material _w3_SkyboxMaterial;
    [SerializeField] private float _w3_LightIntensity = 0.6f;
    [SerializeField] private Color _w3_LightColor = new Color(0.9f, 0.9f, 0.9f);
    
    [Header("Overcast")]
    [SerializeField] private TextMeshProUGUI _txtOvercast;
    [SerializeField] private Material _w4_SkyboxMaterial;
    [SerializeField] private float _w4_LightIntensity = 0.5f;
    [SerializeField] private Color _w4_LightColor = new Color(0.9f, 0.9f, 0.9f);

    [Header("Heavy Rain")]
    [SerializeField] private TextMeshProUGUI _txtHeavyRain;
    [SerializeField] private Material _w5_SkyboxMaterial;
    [SerializeField] private float _w5_LightIntensity = 0.0f;
    [SerializeField] private Color _w5_LightColor = new Color(0.9f, 0.9f, 0.9f);
    [SerializeField] private ParticleSystem _heavyRainParticles;

    [Header("Thunderstorm")]
    [SerializeField] private TextMeshProUGUI _txtThunderstorm;
    [SerializeField] private Material _w6_SkyboxMaterial;
    [SerializeField] private AudioClip _rainSound;
    [SerializeField] private AudioClip _thunderSound;
    [SerializeField] private float _w6_LightIntensity = 0.0f;
    [SerializeField] private Color _w6_LightColor = new Color(0.9f, 0.9f, 0.9f);
    [SerializeField] private ParticleSystem _thunderRainParticles;

    [Header("Clear Morning")]
    [SerializeField] private TextMeshProUGUI _txtClearMorning;
    [SerializeField] private Material _w7_SkyboxMaterial;
    [SerializeField] private float _w7_LightIntensity = 1.0f;
    [SerializeField] private Color _w7_LightColor = Color.white;

    private Coroutine _parkLightningRoutine = null;
    private Coroutine _lightningFlashParkRoutine = null;

    private void OnDestroy()
    {
        ResetAllEffects();
    }

    public void ApplyWeather(WeatherType weatherType, Transform transformParam = null)
    {
        ResetAllEffects();

        if (transformParam != null)
        {
            transform.position = transformParam.position;
            transform.rotation = transformParam.rotation;
            transform.localScale = transformParam.localScale;
        }

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
                ApplyThunderstorm(false);
                break;
            case WeatherType.ThunderstormWithLightning:
                ApplyThunderstorm(true);
                break;
            case WeatherType.ClearMorning:
                ApplyClearMorning();
                break;
            case WeatherType.Default:
                ApplyDefaultWeather();
                break;
            default:
                ApplyDefaultWeather();
                break;
        }
    }

    public void ResetAllEffects()
    {
        if (_txtClearNoon != null)
        {
            _txtClearNoon.gameObject.SetActive(false);
        }

        if (_txtFoggyMorning != null)
        {
            _txtFoggyMorning.gameObject.SetActive(false);
        }

        if (_txtCloudy != null)
        {
            _txtCloudy.gameObject.SetActive(false);
        }

        if (_txtOvercast != null)
        {
            _txtOvercast.gameObject.SetActive(false);
        }

        if (_txtHeavyRain != null)
        {
            _txtHeavyRain.gameObject.SetActive(false);
        }

        if (_txtThunderstorm != null)
        {
            _txtThunderstorm.gameObject.SetActive(false);
        }

        if (_txtClearMorning != null)
        {
            _txtClearMorning.gameObject.SetActive(false);
        }

        transform.position = new Vector3(0, 0, 0);
        transform.rotation = Quaternion.Euler(0, 0, 0);
        transform.localScale = new Vector3(1, 1, 1);

        if (_parkLightningRoutine != null)
        {
            StopCoroutine(_parkLightningRoutine);
        }

        if (_lightningFlashParkRoutine != null)
        {
            StopCoroutine(_lightningFlashParkRoutine);
        }

        RenderSettings.skybox = _defaultSkyBoxMaterial;
        RenderSettings.fog = false;

        if (_directionalLight != null)
        {
            _directionalLight.gameObject.SetActive(true);
            _directionalLight.intensity = 1.0f;
            _directionalLight.color = Color.white;
        }

        if (_sunForWindow != null)
        {
            _sunForWindow.gameObject.SetActive(true);
            _sunForWindow.intensity = 1.0f * _sunForWindowMultiplyer;
            _sunForWindow.color = Color.white;
        }

        if (_heavyRainParticles != null)
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

    private void ApplyClearNoon()
    {
        Debug.Log("Park: Apply Clear Noon");

        if (_txtClearNoon != null)
        {
            _txtClearNoon.gameObject.SetActive(true);
        }

        if (_w1_SkyboxMaterial != null)
        {
            RenderSettings.skybox = _w1_SkyboxMaterial;
        }

        if (_directionalLight != null)
        {
            _directionalLight.intensity = _w1_LightIntensity;
            _directionalLight.color = _w1_LightColor;
        }

        if (_sunForWindow != null)
        {
            _sunForWindow.intensity = _w1_LightIntensity * _sunForWindowMultiplyer;
            _sunForWindow.color = _w1_LightColor;
        }

        RenderSettings.fog = false;
    }

    private void ApplyFoggyMorning()
    {
        Debug.Log("Park: Apply Foggy Morning");

        if (_txtFoggyMorning != null)
        {
            _txtFoggyMorning.gameObject.SetActive(true);
        }

        if (_w2_SkyboxMaterial)
        {
            RenderSettings.skybox = _w2_SkyboxMaterial;
        }

        if (_directionalLight != null)
        {
            _directionalLight.intensity = _w2_LightIntensity;
            _directionalLight.color = _w2_LightColor;
        }

        if (_sunForWindow != null)
        {
            _sunForWindow.intensity = _w2_LightIntensity * _sunForWindowMultiplyer;
            _sunForWindow.color = _w2_LightColor;
        }

        RenderSettings.fog = true;
        RenderSettings.fogDensity = _w2_fogDensity;
    }

    private void ApplyCloudy()
    {
        Debug.Log("Park: Apply Cloudy");

        if (_txtCloudy != null)
        {
            _txtCloudy.gameObject.SetActive(true);
        }

        if (_w3_SkyboxMaterial)
        {
            RenderSettings.skybox = _w3_SkyboxMaterial;
        }

        if (_directionalLight != null)
        {
            _directionalLight.intensity = _w3_LightIntensity;
            _directionalLight.color = _w3_LightColor;
        }

        if (_sunForWindow != null)
        {
            _sunForWindow.intensity = _w3_LightIntensity * _sunForWindowMultiplyer;
            _sunForWindow.color = _w3_LightColor;
        }

        RenderSettings.fog = false;
    }

    private void ApplyOvercast()
    {
        Debug.Log("Park: Apply Overcast");

        if (_txtOvercast != null)
        {
            _txtOvercast.gameObject.SetActive(true);
        }

        if (_w4_SkyboxMaterial)
        {
            RenderSettings.skybox = _w4_SkyboxMaterial;
        }

        if (_directionalLight != null)
        {
            _directionalLight.intensity = _w4_LightIntensity;
            _directionalLight.color = _w4_LightColor;
        }

        if (_sunForWindow != null)
        {
            _sunForWindow.intensity = _w4_LightIntensity * _sunForWindowMultiplyer;
            _sunForWindow.color = _w4_LightColor;
        }

        RenderSettings.fog = false;
    }

    private void ApplyHeavyRain()
    {
        Debug.Log("Park: Apply HeavyRain");

        if (_txtHeavyRain != null)
        {
            _txtHeavyRain.gameObject.SetActive(true);
        }

        if (_w5_SkyboxMaterial)
        {
            RenderSettings.skybox = _w5_SkyboxMaterial;
        }

        if (_directionalLight != null)
        {
            _directionalLight.intensity = _w5_LightIntensity;
            _directionalLight.color = _w5_LightColor;
        }

        if (_sunForWindow != null)
        {
            _sunForWindow.intensity = _w5_LightIntensity * _sunForWindowMultiplyer;
            _sunForWindow.color = _w5_LightColor;
        }

        RenderSettings.fog = false;

        if (_heavyRainParticles != null)
        {
            _heavyRainParticles.Play();
        }
    }

    private void ApplyThunderstorm(bool withLightning)
    {
        Debug.Log("Park: Apply Thunderstorm");

        if (_txtThunderstorm != null)
        {
            _txtThunderstorm.gameObject.SetActive(true);
        }

        if (_w6_SkyboxMaterial)
        {
            RenderSettings.skybox = _w6_SkyboxMaterial;
        }

        if (_directionalLight != null)
        {
            _directionalLight.intensity = _w6_LightIntensity;
            _directionalLight.color = _w6_LightColor;
        }

        if (_sunForWindow != null)
        {
            _sunForWindow.intensity = _w6_LightIntensity * _sunForWindowMultiplyer;
            _sunForWindow.color = _w6_LightColor;
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

        if (withLightning)
        {
            if (_parkLightningRoutine != null)
            {
                StopCoroutine(_parkLightningRoutine);
            }

            _parkLightningRoutine = StartCoroutine(ThunderRoutine(_lightning, _lightningMinInterval, _lightningMaxInterval));
        }
    }

    private void ApplyClearMorning()
    {
        Debug.Log("Park: Apply Clear Morning");

        if (_txtClearMorning != null)
        {
            _txtClearMorning.gameObject.SetActive(true);
        }

        if (_w7_SkyboxMaterial)
        {
            RenderSettings.skybox = _w7_SkyboxMaterial;
        }

        if (_directionalLight != null)
        {
            _directionalLight.intensity = _w7_LightIntensity;
            _directionalLight.color = _w7_LightColor;
        }

        if (_sunForWindow != null)
        {
            _sunForWindow.intensity = _w7_LightIntensity * _sunForWindowMultiplyer;
            _sunForWindow.color = _w7_LightColor;
        }

        RenderSettings.fog = false;
    }

    private void ApplyDefaultWeather()
    {
        Debug.Log("Park: Apply Default Weather");

        ResetAllEffects();
    }

    private IEnumerator ThunderRoutine(Light parkLightning, float minInterval, float maxInterval)
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
            PlayThunder(parkLightning);
        }
    }

    private void PlayThunder(Light parkLightning)
    {
        Debug.Log("PlayThunder");

        if (_lightningFlashParkRoutine != null)
        {
            StopCoroutine(_lightningFlashParkRoutine);
        }

        if (parkLightning != null)
        {
            _lightningFlashParkRoutine = StartCoroutine(LightningFlashParkRoutine(parkLightning));
        }

        if (_ambientAudioSource != null)
        {
            _ambientAudioSource.PlayOneShot(_thunderSound);
        }
    }

    private IEnumerator LightningFlashParkRoutine(Light parkLightning)
    {
        parkLightning.gameObject.SetActive(true);

        if (_sunForWindow != null)
        {
            _sunForWindow.intensity = _lightningSkyboxExposure_1 * _sunForWindowMultiplyer;
            _sunForWindow.color = Color.white;
            _sunForWindow.color = new Color(255.0f, 255.0f, 255.0f, 255.0f);
        }

        if (_w6_SkyboxMaterial != null)
        {
            _w6_SkyboxMaterial.SetFloat("_Exposure", _lightningSkyboxExposure_1);
        }

        if (_roomWindowMaterial != null)
        {
            _roomWindowMaterial.color = new Color(1, 1, 1, 1);
        }

        yield return new WaitForSeconds(_lightningFlashInterval_1);

        parkLightning.gameObject.SetActive(false);

        if (_sunForWindow != null)
        {
            _sunForWindow.intensity = 0.0f;
        }

        if (_w6_SkyboxMaterial != null)
        {
            _w6_SkyboxMaterial.SetFloat("_Exposure", _lightningSkyboxExposure_2);
        }

        if (_roomWindowMaterial != null)
        {
            _roomWindowMaterial.color = new Color(1, 1, 1, 0.02f);
        }

        yield return new WaitForSeconds(_lightningFlashInterval_2);

        parkLightning.gameObject.SetActive(true);

        if (_sunForWindow != null)
        {
            _sunForWindow.intensity = _lightningSkyboxExposure_2 * _sunForWindowMultiplyer;
        }

        if (_w6_SkyboxMaterial != null)
        {
            _w6_SkyboxMaterial.SetFloat("_Exposure", _lightningSkyboxExposure_3);
        }

        if (_roomWindowMaterial != null)
        {
            _roomWindowMaterial.color = new Color(1, 1, 1, 1);
        }

        yield return new WaitForSeconds(_lightningFlashInterval_3);

        parkLightning.gameObject.SetActive(false);

        if (_sunForWindow != null)
        {
            _sunForWindow.intensity = 0.0f;
            _sunForWindow.color = Color.white;
        }

        if (_w6_SkyboxMaterial != null)
        {
            _w6_SkyboxMaterial.SetFloat("_Exposure", _lightningSkyboxExposure_4);
        }

        if (_roomWindowMaterial != null)
        {
            _roomWindowMaterial.color = new Color(1, 1, 1, 0.02f);
        }

        _lightningFlashParkRoutine = null;
    }
}
