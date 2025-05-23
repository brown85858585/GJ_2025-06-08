using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace CharacterSelect
{
    public sealed class SmallIconWidget : MonoBehaviour
    {
        [SerializeField]
        [Header("References")]
        Image icon;
        [SerializeField] TMP_Text level;
        [SerializeField] TMP_Text name;
        [SerializeField] Slider progressBar;
        [SerializeField] Button button;

        public TMP_Text Level => level;
        public TMP_Text Name => name;
        public Image Icon => icon;
        public Slider ProgressBar => progressBar;
        public Button Button => button;

        void Awake()
        {
            button.onClick.AddListener(PlayClickAnimation);
        }

        void PlayClickAnimation()
        {
            icon.transform.DOKill();
            icon.transform.localScale = Vector3.one;
            icon.transform
                .DOPunchScale(Vector3.one * 0.15f, 0.2f, 8, 0.8f);
        }
    }
}