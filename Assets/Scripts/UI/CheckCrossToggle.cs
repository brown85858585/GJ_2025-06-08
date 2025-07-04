using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    [ExecuteAlways]  // Скрипт будет работать и в редакторе
    public class CheckCrossToggle : MonoBehaviour, IPointerClickHandler
    {
        [Header("Sprites")]
        [SerializeField] private Image checkSprite;
        [SerializeField] private Image crossSprite;

        [SerializeField] private bool isOn;

        public bool IsOn
        {
            get => isOn;
            set
            {
                isOn = value;
                UpdateVisual();
            }
        }

        private void Awake()
        {
            // В режиме Play
            UpdateVisual();
        }

        private void OnValidate()
        {
            // Вызывается в редакторе, когда вы меняете isOn в инспекторе
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (checkSprite != null)    checkSprite.gameObject.SetActive(isOn);
            if (crossSprite != null)    crossSprite.gameObject.SetActive(!isOn);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // В режиме Play кликаем мышью
            IsOn = !IsOn;
            OnMusicSettingsChanged?.Invoke(IsOn);
        }

        public event Action<bool> OnMusicSettingsChanged;
    }
}