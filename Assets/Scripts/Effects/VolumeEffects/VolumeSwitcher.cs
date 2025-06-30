using UnityEngine;

namespace Effects.VolumeEffects
{
    public class VolumeSwitcher
    {
        private GameObject _currentEffect;
        private static VolumeSwitcher _instance;
        private static VolumeSwitcherView _viewInstance;
        
        public static VolumeSwitcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new VolumeSwitcher();
                    _viewInstance = Object.FindObjectOfType<VolumeSwitcherView>();
                    if (_viewInstance == null)
                    {
                        Debug.LogError("VolumeSwitcherView instance not found in the scene.");
                    }
                }
                return _instance;
            }
        }
        
        public void SetVolume(VolumeEffectType type)
        {
            if (_currentEffect != null)
            {
                Debug.LogWarning("Volume switcher already set to " + _currentEffect.name);
                return;
            }
            
            if (_viewInstance == null)
            {
                Debug.LogError("VolumeSwitcherView instance is not set.");
                return;
            }

            switch (type)
            {
                case VolumeEffectType.Blur:
                    _currentEffect = Object.Instantiate(_viewInstance.Blur);
                    break;
                case VolumeEffectType.Vignette:
                    _currentEffect = Object.Instantiate(_viewInstance.Vignette);
                    break;
                default:
                    Debug.LogWarning($"Unknown VolumeEffectType: {type}");
                    break;
            }
        }
        
        public void ClearVolume()
        {
            if (_currentEffect != null)
            {
                Object.Destroy(_currentEffect);
                _currentEffect = null;
            }
        }
    }
}