using System;
using System.Collections.Generic;

namespace CharacterSelect
{
    public sealed class SelectorModel : ISelectorModel
    {
        readonly List<CharacterModel> _characters;
        public IReadOnlyList<CharacterModel> Characters => _characters;
        public int SelectedIndex { get; private set; }
        public event Action<int> OnSelectedChanged;

        public SelectorModel(IEnumerable<CharacterModel> chars)
            => _characters = new List<CharacterModel>(chars);

        public void SelectNext() => Select((SelectedIndex + 1) % _characters.Count);
        public void SelectPrev() => Select((SelectedIndex - 1 + _characters.Count) % _characters.Count);

        public void Select(int index)
        {
            if (index == SelectedIndex) return;
            SelectedIndex = index;
            OnSelectedChanged?.Invoke(index);
        }
    }
}