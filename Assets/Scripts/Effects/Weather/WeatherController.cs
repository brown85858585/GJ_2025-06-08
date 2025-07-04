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
        if (Input.GetKeyDown(KeyCode.N))
        {
            WeatherSystem.Instance.NextCycle();
        }
    }
}
