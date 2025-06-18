using System.Collections;
using UnityEngine;

public class ApartmentWeatherEffects : MonoBehaviour, IWeatherEffects
{
    public LocationType Location => LocationType.Apartment;

    [Header("Window Views")]
    [SerializeField] private Renderer[] _windowRenderers;
    
    [Header("Light Effects")]
    // Sun rays through the window
    [SerializeField] private Light _sunLight;
    // Flashes of lightning
    [SerializeField] private Light _lightning;
    [SerializeField] private float _lightningMinInterval = 5.0f;
    [SerializeField] private float _lightningMaxInterval = 15.0f;

    [Header("Audio")]
    [SerializeField] private AudioSource _weatherAudio;

    [Header("Clear Noon")]
    [SerializeField] private Material _w1_WindowViewMaterial;
    [SerializeField] private float _w1_SunLightIntencity = 1.0f;
    [SerializeField] private Color _w1_SunLightColor = Color.white;

    [Header("Foggy Morning")]
    [SerializeField] private Material _w2_WindowViewMaterial;
    [SerializeField] private float _w2_SunLightIntencity = 0.7f;
    [SerializeField] private Color _w2_SunLightColor = new Color(0.9f, 0.9f, 0.8f);

    [Header("Cloudy")]
    [SerializeField] private Material _w3_WindowViewMaterial;

    [Header("Overcast")]
    [SerializeField] private Material _w4_WindowViewMaterial;

    [Header("HeavyRain")]
    [SerializeField] private Material _w5_WindowViewMaterial;
    [SerializeField] private ParticleSystem _rainWindowParticles;
    [SerializeField] private AudioClip _heavyRainWindowSound;

    [Header("Thunderstorm")]
    [SerializeField] private Material _w6_WindowViewMaterial;
    [SerializeField] private AudioClip _rainWindowSound;
    [SerializeField] private AudioClip _thunderSound;

    [Header("ClearMorning")]
    [SerializeField] private Material _w7_WindowViewMaterial;
    [SerializeField] private float _w7_SunLightIntencity = 0.8f;
    [SerializeField] private Color _w7_SunLightColor = new Color(1.0f, 0.95f, 0.9f);

    private Coroutine _apartmentLightningRoutine;
    private Coroutine _lightningFlashApartmentRoutine;

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
                ApplyThunderstorm();
                break;
            case WeatherType.ClearMorning:
                ApplyClearMorning();
                break;
        }
    }

    public void ResetAllEffects()
    {
        transform.position = new Vector3(0, 0, 0);
        transform.rotation = Quaternion.Euler(0, 0, 0);
        transform.localScale = new Vector3(1, 1, 1);

        //Lightning.Instance.StopApartmentLightning();

        if (_lightningFlashApartmentRoutine != null)
        {
            StopCoroutine(_lightningFlashApartmentRoutine);
        }

        if (_apartmentLightningRoutine != null)
        {
            StopCoroutine(_apartmentLightningRoutine);
        }

        if (_sunLight != null)
        {
            _sunLight.gameObject.SetActive(false);
        }

        if (_lightning != null)
        {
            _lightning.gameObject.SetActive(false);
        }

        if (_weatherAudio != null)
        {
            _weatherAudio.Stop();
        }
    }

    private void ApplyClearNoon()
    {
        Debug.Log("Apartrment: Apply Clear Noon");

        Debug.Log(_lightning);

        if (_windowRenderers != null && _windowRenderers.Length > 0 && _w1_WindowViewMaterial != null)
        {
            foreach (Renderer windowRenderer in _windowRenderers)
            {
                windowRenderer.material = _w1_WindowViewMaterial;
            }
        }

        if (_sunLight != null)
        {
            _sunLight.gameObject.SetActive(true);
            _sunLight.intensity = _w1_SunLightIntencity;
            _sunLight.color = _w1_SunLightColor;
        }
    }

    private void ApplyFoggyMorning()
    {
        Debug.Log("Apartrment: Apply Foggy Morning");

        Debug.Log(_lightning);

        if (_windowRenderers != null && _windowRenderers.Length > 0 && _w2_WindowViewMaterial != null)
        {
            foreach (Renderer windowRenderer in _windowRenderers)
            {
                windowRenderer.material = _w2_WindowViewMaterial;
            }
        }

        if (_sunLight != null)
        {
            _sunLight.gameObject.SetActive(true);
            _sunLight.intensity = _w2_SunLightIntencity;
            _sunLight.color = _w2_SunLightColor;
        }
    }

    private void ApplyCloudy()
    {
        Debug.Log("Apartrment: Apply Cloudy");

        Debug.Log(_lightning);

        if (_windowRenderers != null && _windowRenderers.Length > 0 && _w3_WindowViewMaterial != null)
        {
            foreach (Renderer windowRenderer in _windowRenderers)
            {
                windowRenderer.material = _w3_WindowViewMaterial;
            }
        }

        if (_sunLight != null)
        {
            _sunLight.gameObject.SetActive(false);
        }
    }

    private void ApplyOvercast()
    {
        Debug.Log("Apartrment: Apply Overcast");

        Debug.Log(_lightning);

        if (_windowRenderers != null && _windowRenderers.Length > 0 && _w4_WindowViewMaterial != null)
        {
            foreach (Renderer windowRenderer in _windowRenderers)
            {
                windowRenderer.material = _w4_WindowViewMaterial;
            }
        }

        if (_sunLight != null)
        {
            _sunLight.gameObject.SetActive(false);
        }
    }

    private void ApplyHeavyRain()
    {
        Debug.Log("Apartrment: Apply HeavyRain");

        Debug.Log(_lightning);

        if (_windowRenderers != null && _windowRenderers.Length > 0 && _w5_WindowViewMaterial != null)
        {
            foreach (Renderer windowRenderer in _windowRenderers)
            {
                windowRenderer.material = _w5_WindowViewMaterial;
            }
        }

        if (_sunLight != null)
        {
            _sunLight.gameObject.SetActive(false);
        }

        if (_rainWindowParticles != null)
        {
            _rainWindowParticles.Play();
        }

        if (_weatherAudio != null && _heavyRainWindowSound != null)
        {
            _weatherAudio.clip = _heavyRainWindowSound;
            _weatherAudio.Play();
        }
    }

    private void ApplyThunderstorm()
    {
        Debug.Log("Apartrment: Apply Thunderstorm");

        if (_windowRenderers != null && _windowRenderers.Length > 0 && _w6_WindowViewMaterial != null)
        {
            foreach (Renderer windowRenderer in _windowRenderers)
            {
                windowRenderer.material = _w6_WindowViewMaterial;
            }
        }

        if (_sunLight != null)
        {
            _sunLight.gameObject.SetActive(false);
        }

        if (_weatherAudio != null && _rainWindowSound != null)
        {
            _weatherAudio.clip = _rainWindowSound;
            _weatherAudio.Play();
        }

        if (_lightning != null)
        {
            //Lightning.Instance.StartApartmentLightning(_lightning, _lightningMinInterval, _lightningMaxInterval);
            if (_apartmentLightningRoutine != null)
            {
                StopCoroutine(_apartmentLightningRoutine);
            }

            _apartmentLightningRoutine = StartCoroutine(LightningApartmentRoutine(_lightning, _lightningMinInterval, _lightningMaxInterval));
        }
        else
        {
            Debug.Log("_lightning is null!");
        }
    }

    private void ApplyClearMorning()
    {
        Debug.Log("Apartrment: Apply Clear Morning");

        if (_windowRenderers != null && _windowRenderers.Length > 0 && _w7_WindowViewMaterial != null)
        {
            foreach (Renderer windowRenderer in _windowRenderers)
            {
                windowRenderer.material = _w7_WindowViewMaterial;
            }
        }

        if (_sunLight != null)
        {
            _sunLight.gameObject.SetActive(true);
            _sunLight.intensity = _w7_SunLightIntencity;
            _sunLight.color = _w7_SunLightColor;
        }
    }

    private IEnumerator LightningApartmentRoutine(Light apartmentLightning, float minInterval, float maxInterval)
    {
        Debug.Log("LightningApartmentRoutine()");

        while (this != null)
        {
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
            PlayThunder(apartmentLightning);
        }
    }

    private void PlayThunder(Light apartmentLightning)
    {
        Debug.Log("PlayThunder()");

        if (apartmentLightning != null)
        {
            if (_lightningFlashApartmentRoutine != null)
            {
                StopCoroutine(_lightningFlashApartmentRoutine);
            }

            _lightningFlashApartmentRoutine = StartCoroutine(LightningFlashApartmentRoutine(apartmentLightning));
        }

        if (_weatherAudio != null)
        {
            _weatherAudio.PlayOneShot(_thunderSound);
        }
    }

    private IEnumerator LightningFlashApartmentRoutine(Light apartmentLightning)
    {
        Debug.Log("PlayApartmentLightning()");

        apartmentLightning.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        apartmentLightning.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.05f);
        apartmentLightning.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        apartmentLightning.gameObject.SetActive(false);

        _lightningFlashApartmentRoutine = null;
    }
}
