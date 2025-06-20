using UnityEngine;

public class WeatherController : MonoBehaviour
{
    private LocationType _currentLocationType;

    private void Start()
    {
        _currentLocationType = LocationType.Park;
        WeatherSystem.Instance.SetLocation(_currentLocationType);
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.L))
        //{
        //    ToggleLocation();
        //}

        if (Input.GetKeyDown(KeyCode.N))
        {
            WeatherSystem.Instance.NextCycle();
        }
    }

//    private void ToggleLocation()
//    {
//        if (_currentLocationType == LocationType.Apartment)
//        {
//            _currentLocationType = LocationType.Park;
//        }
//        else
//        {
//            _currentLocationType = LocationType.Apartment;
//        }

//        WeatherSystem.Instance.SetLocation(_currentLocationType);

//        Debug.Log("New location: " + WeatherSystem.Instance.CurrentLocation);
//;    }
}
