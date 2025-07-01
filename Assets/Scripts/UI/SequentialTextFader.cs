using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

namespace UI
{
    public class SequentialTextFader : MonoBehaviour
    {
        [Header("Тексты (в нужном порядке)")]
        [SerializeField] private List<TMP_Text> texts = new();

        [Header("Параметры анимации")]
        [SerializeField, Min(0f)]
        private float fadeDuration = 0.5f;   // cкорость появления (сек)
    
        [SerializeField, Min(0f)]
        private float interval = 2f;  
        
        public event Action OnComplete;  // событие завершения анимации
        private void Awake()
        {
            // стартуем с полной прозрачности
            foreach (var t in texts)
            {
                if (t == null) continue;
                var c = t.color;
                c.a = 0;
                t.color = c;
            }
        }

        private void OnDisable()
        {
            foreach (var t in texts)
            {
                if (t == null) continue;
                var c = t.color;
                c.a = 0;
                t.color = c;
            }
        }

        private void OnEnable()
        {
            PlaySequence();
        }

        private void PlaySequence()
        {
            // Строим DOTween-последовательность: [fade-in] → [интервал] → …
            Sequence seq = DOTween.Sequence();

            foreach (var t in texts)
            {
                if (t == null) continue;

                // гарантируем, что альфа 0 (на случай переиспользования)
                seq.Append(t.DOFade(0f, 0f));

                // плавное появление
                seq.Append(t.DOFade(1f, fadeDuration)
                    .SetEase(Ease.InOutQuad)
                    .SetLink(t.gameObject));

                // пауза перед следующим
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