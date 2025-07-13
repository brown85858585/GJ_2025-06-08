using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Utilities;
using Sequence = DG.Tweening.Sequence;

namespace UI
{
    public class SequentialTextFader : MonoBehaviour
    {
        [Header("Тексты (в нужном порядке)")]
        [SerializeField] private List<TMP_Text> texts = new();
        
        [SerializeField] private UIElementTweener tweener;

        [Header("Параметры анимации")]
        [SerializeField, Min(0f)]
        private float fadeDuration = 0.5f;   // cкорость появления (сек)
    
        [SerializeField, Min(0f)]
        private float interval = 2f;  
        
        public event Action OnComplete;  // событие завершения анимации

        public float TotalAnimationDuration => texts.Count * (fadeDuration + interval);

        public event Action<int> OnTextRevealed;

        private void OnDisable()
        {
            // стартуем с полной прозрачности
            foreach (var t in texts)
            {
                if (t == null) continue;
                var c = t.color;
                c.a = 0;
                t.color = c;
            }
            
            if (tweener == null) return;
            tweener.Hide();
            OnComplete -= tweener.Show;  // отписываемся от события показа
        }

        private void OnEnable()
        {
            // стартуем с полной прозрачности
            foreach (var t in texts)
            {
                if (t == null) continue;
                var c = t.color;
                c.a = 0;
                t.color = c;
            }
            if (tweener != null)
            {
                tweener.Hide();
            }
            
            
            PlaySequence();
            
            
            
            if (tweener == null) return;
            OnComplete += tweener.Show;  // подписываемся на событие показа
        }

        private void PlaySequence()
        {
            // Строим DOTween-последовательность: [fade-in] → [интервал] → …
            Sequence seq = DOTween.Sequence();
            int textIndex = 0;

            foreach (var t in texts)
            {
                if (t == null) continue;

                seq.Append(t.DOFade(0f, 0f));
                seq.Append(t.DOFade(1f, fadeDuration)
                    .SetEase(Ease.InOutQuad)
                    .SetLink(t.gameObject)
                    .OnComplete(() => OnTextRevealed?.Invoke(textIndex++))); // Событие для каждого текста

                seq.AppendInterval(interval);
            }
            // Завершаем последовательность с событием

            seq.OnComplete(() =>
            {
                OnComplete?.Invoke();  // вызываем событие завершения
            });

        }
    }
}