using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterSelect
{
    public class CharacterSelectView : UIElementTweener, ICharacterSelectView
    {
        [SerializeField] private Button backButton;
        [SerializeField] private Image bigIcon;
        [SerializeField] private Slider bigIconSlider;
        [SerializeField] private Transform smallIconsParent;
        [SerializeField] private GameObject smallIconPrefab; 
    
        public event Action<int> OnCharacterButtonClicked = delegate { };
        public event Action OnBackClicked = delegate { };
        private readonly List<SmallIconWidget> _widgets = new();
        
        private void Awake()
        {
            backButton.onClick.AddListener(OnBackButtonClick);
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            backButton.onClick.RemoveAllListeners();
            foreach (var widget in _widgets)
            {
                widget.Button.onClick.RemoveAllListeners();
            }
            _widgets.Clear();
            
        }
        
        private void OnEnable()
        {
            Show();
            
        }
    
        private void OnDisable()
        {
            Hide();
        }

        private void OnBackButtonClick()
        {
            OnBackClicked?.Invoke();
        }


        public void Build(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var go = Instantiate(smallIconPrefab, smallIconsParent);
                int captured = i;
                var widget = go.GetComponent<SmallIconWidget>();
                widget.Button.onClick.AddListener(() => OnCharacterButtonClicked?.Invoke(captured));
                _widgets.Add(widget);
            }
        }

        public void SetSelectedBigIcon(Sprite icon, float v) => bigIcon.sprite = icon;
        public void SetSmallIconLevel(int i, string label) => _widgets[i].Level.text = label;
        public void SetSmallIconName(int i, string name) => _widgets[i].Name.text = name;
        public void SetSmallIcon(int i, Sprite icon) => _widgets[i].Icon.sprite = icon;
        public void SetProgress(int i, float v) => _widgets[i].ProgressBar.value = v;

        public void AnimateSwitch(int to, float currentValue)
        {
            var seq = DOTween.Sequence();
            seq.Append(bigIcon.DOFade(0f, .15f));
            seq.Join(bigIcon.transform.DOScale(.8f, .15f));
            seq.AppendCallback(() => bigIcon.sprite = _widgets[to].Icon.sprite);
            seq.Append(bigIcon.DOFade(1f, .15f));
            seq.Join(bigIcon.transform.DOScale(1f, .15f));
            seq.AppendCallback(() => bigIconSlider.value = bigIconSlider.minValue);
            seq.Append(bigIconSlider
                    .DOValue(currentValue, .30f)
                    .SetEase(Ease.Linear));
        }
    }
}
