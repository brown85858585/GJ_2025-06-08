using System;
using Game.MiniGames.Park;
using Player;

namespace Game.MiniGames
{
    public class ParkSprintController : IDisposable
    {
        private readonly ParkLevelView _parkLevelView;
        private readonly IPlayerController _playerController;
        
        private int _currentRingIndex;

        public event Action EndSprint; 

        public ParkSprintController(IPlayerController playerController, ParkLevelView parkLevelView)
        {
            _parkLevelView = parkLevelView;
            _playerController = playerController;
            
            _parkLevelView.OnRingEntered += HandleRingEntered;
        }

        private void HandleRingEntered(int id)
        {
            if (id != _currentRingIndex || id >= _parkLevelView.Rings.Count-1)
            {
                EndSprint?.Invoke();
                return;
            }
            
            _currentRingIndex++;
        }

        public void Dispose()
        {
            _parkLevelView.OnRingEntered -= HandleRingEntered;
        }
    }
}