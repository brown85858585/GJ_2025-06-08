using System;

namespace CharacterSelect
{
    public sealed class CharacterSelectPresenter : IDisposable
    {
        private readonly ICharacterSelectView _view;
        private readonly ISelectorModel _model;

        public CharacterSelectPresenter(ICharacterSelectView view, ISelectorModel model)
        {
            _view  = view;
            _model = model;

            // init UI
            _view.Build(_model.Characters.Count);
            for (int i = 0; i < _model.Characters.Count; i++)
            {
                var characterModel = _model.Characters[i];
                _view.SetSmallIcon(i, characterModel.Data.icon);
                _view.SetSmallIconLevel(i, characterModel.Data.level.ToString());
                _view.SetSmallIconName(i, characterModel.Data.name);
                _view.SetProgress(i, characterModel.Xp / (float)characterModel.GetXpToNextLevel());
                characterModel.OnXpChanged += (_, _) => RefreshProgress(i);
            }
            RefreshSelection(0);

            // subscribe UI events
            _view.OnCharacterButtonClicked += OnUiSelect;
            _view.OnBackClicked            += HandleBack;
            _model.OnSelectedChanged       += modelIndex => RefreshSelection(modelIndex);
        }

        void OnUiSelect(int idx) => _model.Select(idx);
        
        void RefreshSelection(int newIndex)
        {
            var sprite = _model.Characters[newIndex].Data.icon;
            var cm = _model.Characters[newIndex];
            var currentValue = cm.Xp / (float)cm.GetXpToNextLevel();
            
            _view.AnimateSwitch(newIndex, currentValue);
            _view.SetSelectedBigIcon(sprite, currentValue);
        }

        void RefreshProgress(int idx)
        {
            var cm = _model.Characters[idx];
            _view.SetProgress(idx, cm.Xp / (float)cm.GetXpToNextLevel());
        }

        void HandleBack()
        {
            _view.Hide();            
        }

        public void Dispose()
        {
            _view.OnCharacterButtonClicked -= OnUiSelect;
            _view.OnBackClicked            -= HandleBack;
            _model.OnSelectedChanged       -= RefreshSelection;
        }
    }

}