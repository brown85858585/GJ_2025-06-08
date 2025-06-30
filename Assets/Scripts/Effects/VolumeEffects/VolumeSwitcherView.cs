using UnityEngine;

namespace Effects.VolumeEffects
{
    public class VolumeSwitcherView : MonoBehaviour
    {
        [SerializeField] private GameObject blurVolume;
    
        public GameObject Blur => blurVolume;
    }
}
