using System;
using Player;
using UnityEngine;

namespace Game.Installers
{
    public class ScenarioInstaller : MonoBehaviour
    {
        private IPlayerController _playerController;
        
        public IPlayerController PlayerController => _playerController;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Initialize(IPlayerController playerController)
        {
            _playerController = playerController;
        }
        public void NextLevelScenario(Action nextLevelStart)
        {
            nextLevelStart?.Invoke();
        }
    }
}