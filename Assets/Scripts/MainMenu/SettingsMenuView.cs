using System;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu
{
    public class SettingsMenuView : MonoBehaviour
    {
        [SerializeField] private Button _settingsBackButton;
        [SerializeField] private CheckCrossToggle toggleMusic;
        
        private void Start()
        {
            toggleMusic.OnMusicSettingsChanged += ToggleMusic;

            toggleMusic.IsOn = MusicManager.Instance.IsOn;
        }

        private void ToggleMusic(bool isOn)
        {
            MusicManager.Instance.ToggleSound(isOn);
        }

        public Button SettingsBackButton => _settingsBackButton;

        private void OnDestroy()
        {
            toggleMusic.OnMusicSettingsChanged -= ToggleMusic;
        }
    }
}