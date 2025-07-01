using System;
using Game.MiniGames.Park;
using Player;
using UnityEngine;

namespace Game.MiniGames
{
    public class ParkSprintController : IDisposable
    {
        private readonly ParkLevelView _parkLevelView;
        private readonly IPlayerController _playerController;

        private int _staminaMultiplyer = 40;
        private int _staminaMax = 10000;
        private float _floatDelta = 0.001f;
        private int _staminaForRing = 2000;
        private bool _isFirstStaminaUpdateView = true;

        public event Action<bool> EndSprint; 

        public ParkSprintController(IPlayerController playerController, ParkLevelView parkLevelView)
        {
            _parkLevelView = parkLevelView;
            _playerController = playerController;

            _parkLevelView.OnRingEntered += HandleRingEntered;
            _parkLevelView.OnStaminaChanged += HandleStaminaChanged;
        }

        private void HandleRingEntered(int id)
        { 
            _playerController.Model.Score += 100;
            AddStamina(_staminaForRing);
            
            Debug.Log(_parkLevelView.CheckpointCounter);
            if(_parkLevelView.CheckpointCounter == 0)
            {
                EndGame();
            }
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
                EndGame();
            }
        }

        private void EndGame()
        {
            _parkLevelView.OnStaminaChanged -= HandleStaminaChanged;
            var win = _parkLevelView.CheckpointCounter < _parkLevelView.CheckpointViews.Count/2;
            EndSprint?.Invoke(win);

            ResetStamina();
        }
        
        private void ResetStamina()
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