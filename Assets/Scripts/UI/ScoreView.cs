using System;
using DG.Tweening;                 // ← главное: подключаем DOTween
using TMPro;
using UnityEngine;

namespace Game.Installers
{
    public class ScoreView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text scoreTextShadow;
        [SerializeField] private TMP_Text deltaText;

        [Header("Animation")]
        [SerializeField] private float scorePunchScale   = 0.2f;   // амплитуда «пинка» счёта
        [SerializeField] private float scorePunchTime    = 0.35f;  // длительность
        [SerializeField] private float deltaScaleInTime  = 0.25f;  // насколько быстро дельта «вылетает»
        [SerializeField] private float deltaStayTime     = 1.0f;   // сколько дельта держится
        [SerializeField] private float deltaFadeOutTime  = 0.4f;   // сколько исчезает

        private int  _lastScore = 0;
        private Tween _deltaTween;
        private Tween _scoreTween;

        public int Score
        {
            get => int.Parse(scoreText.text);
            set => SetScore(value);
        }

        private void SetScore(int score)
        {
            // --- счёт и тень --------------------------------------------------
            scoreText.text        = score.ToString();
            scoreTextShadow.text  = score.ToString();

            // «пинок» счёта
            _scoreTween?.Kill();
            scoreText.transform.localScale = Vector3.one;   // сброс
            _scoreTween = scoreText.transform
                .DOPunchScale(Vector3.one * scorePunchScale, scorePunchTime, vibrato: 5, elasticity: 0.6f)
                .SetUpdate(true);

            // --- дельта -------------------------------------------------------
            int delta = score - _lastScore;
            if (delta != 0)
            {
                deltaText.color = delta > 0 ? Color.green : Color.red;
                deltaText.text  = (delta > 0 ? "+" : "") + delta.ToString();
            }
            else
            {
                deltaText.color = Color.white;
                deltaText.text  = "0";
            }

            // анимация дельты: выскочила, подержалась, исчезла
            _deltaTween?.Kill();
            deltaText.transform.localScale = Vector3.zero;
            deltaText.alpha                = 0;

            _deltaTween = DOTween.Sequence()
                .Append(deltaText.transform
                    .DOScale(Vector3.one, deltaScaleInTime)
                    .SetEase(Ease.OutBack))
                .Join(deltaText.DOFade(1f, deltaScaleInTime))
                .AppendInterval(deltaStayTime)
                .Append(deltaText.DOFade(0f, deltaFadeOutTime))
                .Join(deltaText.transform
                    .DOScale(Vector3.zero, deltaFadeOutTime)
                    .SetEase(Ease.InBack))
                .SetUpdate(true);

            _lastScore = score;
        }

        private void OnDestroy()     // не забываем чиститься
        {
            _deltaTween?.Kill();
            _scoreTween?.Kill();
        }
    }
}