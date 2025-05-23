using UnityEngine;
using UnityEngine.UI;

namespace MainMenu
{
    [RequireComponent(typeof(Button))]
    public class MenuButton : MonoBehaviour
    {
        [SerializeField] private MenuBtnType buttonType;
        private Button _button;

        public MenuBtnType ButtonType => buttonType;
        public Button Button => _button;

        private void Awake()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }
        }
    }
}