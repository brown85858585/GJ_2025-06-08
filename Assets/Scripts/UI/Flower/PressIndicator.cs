using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UI.Flower
{
    public class PressIndicator : MonoBehaviour
    {
        [SerializeField] private RectTransform iconButton;
        [SerializeField] private Image mySlider;
    
        [Header("Pulse Settings")]
        [SerializeField, Min(0f)] private float pulseScale = 1.2f;
        [SerializeField, Min(0f)] private float pulseDuration = 1f;

        [Header("Press E Settings")]
        [SerializeField, Min(0f)] private float maxScale = 1.2f;

        [Header("Timer Settings")]
        [SerializeField, Min(0f)] private float maxTimer = 10f;  // Максимальное значение таймера

        [Range(0f, 1f)]
        [SerializeField] private float multiplier = 0.1f; // Скорость уменьшения значения слайдера
    
        private float _baseScale;
        private float _animationTime;
        private bool _isCompleted;
        
        public event Action<bool> OnCompleteIndicator;
        
        private void Awake()
        {
            _baseScale = iconButton.localScale.x;
        }

        private void Update()
        {
            if (mySlider.fillAmount is >= 0.99f or <= 0)
            {
                return;
            }
            
            mySlider.fillAmount -= Time.deltaTime * multiplier;
            
            // Анимация пульсации
            _animationTime += Time.deltaTime;
            float sine = Mathf.Sin((_animationTime / pulseDuration) * Mathf.PI * 2f) * 0.5f + 0.5f;
            float currentScale = Mathf.Lerp(_baseScale, _baseScale * pulseScale, sine);
            iconButton.localScale = Vector3.one * currentScale;
        }

        private void FixedUpdate()
        {
            CheckCompletedIndicator();
        }

        private void CheckCompletedIndicator()
        {   
            if (_isCompleted)
            {
                return;
            }
            
            if (mySlider.fillAmount >= 1f)
            {
                OnCompleteIndicator?.Invoke(true);
                _isCompleted = true;
            }

            if (mySlider.fillAmount <= 0)
            {
                OnCompleteIndicator?.Invoke(false);
                _isCompleted = true;
            }
        }

        public void HandleInteract()
        {
            if(!gameObject.activeInHierarchy) return;
            // Сброс анимации пульсации
            _animationTime = 0;

            // Увеличение значения слайдера 

            mySlider.fillAmount += Random.Range(0.02f, 0.05f);
        }

        public void SetMultiplier(float newMultiplier)
        {
            multiplier = newMultiplier;
        }
    }
}
