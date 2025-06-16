using System.Collections;
using UnityEngine;

public class Lightning : MonoBehaviour
{
    public static Lightning Instance { get; private set; }

    private Coroutine _apartmentLightningRoutine;
    private Coroutine _lightningFlashApartmentRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartApartmentLightning(Light apartmentLightning, float minInterval, float maxInterval)
    {
        StopApartmentLightning();
        _apartmentLightningRoutine = StartCoroutine(LightningApartmentRoutine(apartmentLightning, minInterval, maxInterval));
    }

    public void StopApartmentLightning()
    {
        if (_lightningFlashApartmentRoutine != null)
        {
            StopCoroutine(_lightningFlashApartmentRoutine);
        }

        if (_apartmentLightningRoutine != null)
        {
            StopCoroutine(_apartmentLightningRoutine);
        }
    }

    private IEnumerator LightningApartmentRoutine(Light apartmentLightning, float minInterval, float maxInterval)
    {
        Debug.Log("LightningApartmentRoutine()");

        while (this != null)
        {
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
            PlayApartmentLightning(apartmentLightning);
        }
    }

    private void PlayApartmentLightning(Light apartmentLightning)
    {
        Debug.Log("PlayApartmentLightning()");

        if (apartmentLightning != null)
        {
            if (_lightningFlashApartmentRoutine != null)
            {
                StopCoroutine(_lightningFlashApartmentRoutine);
            }

            _lightningFlashApartmentRoutine = StartCoroutine(LightningFlashApartmentRoutine(apartmentLightning));
        }

        //if (_weatherAudio != null)
        //{
        //    _weatherAudio.PlayOneShot(_thunderSound);
        //}
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
