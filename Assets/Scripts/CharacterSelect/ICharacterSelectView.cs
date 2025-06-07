using System;
using UnityEngine;

namespace CharacterSelect
{
    public interface ICharacterSelectView
    {
        event Action<int>  OnCharacterButtonClicked;
        event Action       OnBackClicked;
        
        public void Build(int count);
        void SetSelectedBigIcon(Sprite icon, float v);
        void SetSmallIcon(int index, Sprite icon);
        public void SetSmallIconLevel(int i, string label);
        public void SetSmallIconName(int i, string name);
        void SetProgress(int index, float normalizedValue);
        void AnimateSwitch(int to, float currentValue);
        void Show();
        void Hide();
    }
}