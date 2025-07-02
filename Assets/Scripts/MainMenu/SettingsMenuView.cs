using UnityEngine;
using UnityEngine.UI;

namespace MainMenu
{
    public class SettingsMenuView : MonoBehaviour
    {
        [SerializeField] private Button _settingsBackButton;

        public Button SettingsBackButton => _settingsBackButton;
    }
}