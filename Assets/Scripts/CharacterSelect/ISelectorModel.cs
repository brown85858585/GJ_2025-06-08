using System;
using System.Collections.Generic;

namespace CharacterSelect
{
    public interface ISelectorModel
    {
        IReadOnlyList<CharacterModel> Characters { get; }
        int SelectedIndex   { get; }
        
        event Action<int> OnSelectedChanged;
        void SelectNext();
        void SelectPrev();
        void Select(int index);
    }
}