using System;
using System.Diagnostics;
using Game.MiniGames.Park;
using Player;

namespace Game.MiniGames
{
    public class ParkSprintController : IDisposable
    {
        private readonly ParkLevelView _parkLevelView;
        private readonly IPlayerController _playerController;
        
        private int _currentRingIndex;

        private int _staminaMultiplyer = 40;
        private int _staminaMax = 10000;
        private float _floatDelta = 0.001f;
        private int _staminaForRing = 2000;
        private bool _isFirstStaminaUpdateView = true;

        public event Action EndSprint; 

        public ParkSprintController(IPlayerController playerController, ParkLevelView parkLevelView)
        {
            _parkLevelView = parkLevelView;
            _playerController = playerController;

            _parkLevelView.OnRingEntered += HandleRingEntered;
            _parkLevelView.OnStaminaChanged += HandleStaminaChanged;
        }

        private void HandleRingEntered(int id)
        {
            if (id != _currentRingIndex || id >= _parkLevelView.CheckpointViews.Count-1)
            {
                EndSprint?.Invoke();
                return;
            }

            _currentRingIndex++;
            AddStamina(_staminaForRing);
        }

        private void AddStamina(int value)
        {
            _playerController.Model.Stamina += value;
            _parkLevelView.UpdateStaminaView(_playerController.Model.Stamina);
        }

        private void HandleStaminaChanged()
        {
            if (!_isFirstStaminaUpdateView && _playerController.Movement.NormalizedSpeed < _floatDelta)
            {
                return;
            }

            _isFirstStaminaUpdateView = false;

            _playerController.Model.Stamina -= (int)(_playerController.Movement.NormalizedSpeed * _staminaMultiplyer);

            _parkLevelView.UpdateStaminaView(_playerController.Model.Stamina);

            if (_playerController.Model.Stamina == 0)
            { 
                _parkLevelView.OnStaminaChanged -= HandleStaminaChanged;
                EndSprint?.Invoke();
                StaminaReset();
            }
        }

        private void StaminaReset()
        {
            _isFirstStaminaUpdateView = true;
            _playerController.Model.Stamina = _staminaMax;
        }

        public void Dispose()
        {
            _parkLevelView.OnStaminaChanged -= HandleStaminaChanged;
            _parkLevelView.OnRingEntered -= HandleRingEntered;
        }
    }
}