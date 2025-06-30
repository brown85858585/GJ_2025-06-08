using UnityEngine;

namespace Effects.VolumeEffects
{
    public class VolumeSwitcherView : MonoBehaviour
    {
        [SerializeField] private GameObject blurVolume;
        [SerializeField] private GameObject vignetteVolume;
    
        public GameObject Blur => blurVolume;
        public GameObject Vignette => vignetteVolume;
    }
}
