using System;
using Effects.PostProcess;
using Game.Interactions;
using Game.MiniGames;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Levels
{
    public class LevelManager
    {
        private readonly LevelsConfig _config;
        private readonly InteractionSystem _interactionSystem;
        private readonly MiniGameCoordinator _miniGameCoordinator;
        private GameObject _currentLevel;
        private int _currentIndex;
        private EffectAccumulatorView _effectAccumulatorView;
        public event Action<int> OnNextLevelLoaded;

        public int CurrentLevelIndex => _currentIndex;
        public RoomView CurrentRoomView { get; private set; }

        public LevelManager(LevelsConfig config,
            InteractionSystem interactionSystem,
            MiniGameCoordinator miniGameCoordinator)
        {
            _config = config;
            _interactionSystem = interactionSystem;
            _miniGameCoordinator = miniGameCoordinator;
            _currentIndex = -1;
        }

        /// <summary>
        /// Загружает уровень по индексу из LevelsConfig.
        /// </summary>
        public void LoadLevel(int index, Transform parent, EffectAccumulatorView effectAccumulatorView = null)
        {
            OnNextLevelLoaded?.Invoke(index);
            
            if (effectAccumulatorView != null)
            {
                _effectAccumulatorView = effectAccumulatorView;
            }
            // Убираем старый уровень
            if (_currentLevel != null)
            {
                _interactionSystem.ClearAll();   // очищает все собранные коллекции
                _miniGameCoordinator.UnregisterAll();
                Object.Destroy(_currentLevel);
            }

            // Загружаем новый
            _currentIndex = index;


            var prefab = _config.levels[index].levelPrefab;
            _currentLevel = Object.Instantiate(prefab, parent);
            CurrentRoomView = _currentLevel.GetComponentInChildren<RoomView>();

            // Регистрируем взаимодействия и мини-игры
            _interactionSystem.AddNewInteractionCollection(CurrentRoomView);
            _miniGameCoordinator.SetLevel(index);
            _miniGameCoordinator.RegisterGames(_currentLevel.transform, _effectAccumulatorView);
           
        }

        /// <summary>
        /// Загружает следующий уровень, если он есть
        /// </summary>
        public bool LoadNextLevel(Transform parent)
        {
            int next = _currentIndex + 1;
            if (next < _config.levels.Length)
            {
                switch (next)
                {
                    case 0:
                    case 1:
                        MusicManager.Instance.SetTrack(MusicTrack.Track1);
                        break;
                    case 2:
                        MusicManager.Instance.SetTrack(MusicTrack.Track2);
                        break;
                    case 3: 
                    case 4: 
                        MusicManager.Instance.SetTrack(MusicTrack.Track3);
                        break;
                    case 5: 
                    case 6: 
                        MusicManager.Instance.SetTrack(MusicTrack.Track4);
                        break;
                    case 7: 
                        MusicManager.Instance.SetTrack(MusicTrack.Track5);
                        break;
                }

                LoadLevel(next, parent);
                return true;
            }
            return false;
        }
    }
}