using System;
using Cysharp.Threading.Tasks;
using Effects;
using Effects.VolumeEffects;
using Game.MiniGames.Park;
using Game.Quests;
using Player;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.MiniGames
{
    public class ParkMiniGame : IMiniGame
    {
        public int Level { get ; set ; }
        public bool IsCompleted { get; set; }
        public bool IsWin { get; private set; }
        public event Action<QuestType> OnMiniGameStart;
        public event Action<QuestType> OnMiniGameComplete;
        public QuestType QType { get; } = QuestType.Sprint;

        private Transform _levelRoom;
        private ParkLevelView _parkLevelView;
        private ParkSprintController _parkSprintController;
        private readonly IPlayerController _playerController;
        private EffectAccumulatorView _effectsAccumulatorView;
        private int _savedScore;

        public ParkMiniGame(IPlayerController playerController)
        {
            _playerController = playerController;
        }

        public void Initialization(ParkLevelView parkView, EffectAccumulatorView effectAccumulatorView, Transform level)
        {
            _parkLevelView = parkView;
            _parkLevelView.gameObject.SetActive(false);
            _parkLevelView.transform.position = Vector3.up * 5f;
            _levelRoom = level;
            _effectsAccumulatorView = effectAccumulatorView;
        }
        public void StartGame()
        {
            _savedScore = _playerController.Model.Score;
            
            _effectsAccumulatorView.FadeIn();
            UniTask.Delay(1000).ContinueWith(() =>
            {
                _parkLevelView.gameObject.SetActive(true);
                _playerController.SetPosition(Vector3.up * 5.1f);
                _playerController.ToggleMovement();
                
                DisableLevelInNextFrame().Forget();
                
                _parkSprintController = new ParkSprintController(_playerController, _parkLevelView);
                _parkSprintController.EndSprint += (win) =>
                {
                    RunCompletingTimer(win).Forget();
                };
                
                _effectsAccumulatorView.FadeOut();
                VolumeSwitcher.Instance.SetVolume(VolumeEffectType.Vignette);
            });
        }

        private async UniTask DisableLevelInNextFrame()
        {
            await UniTask.WaitForFixedUpdate();
            _levelRoom.gameObject.SetActive(false);
        }

        private async UniTask RunCompletingTimer(bool win)
        {
            _playerController.ToggleMovement();
            
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            _effectsAccumulatorView.FadeIn();
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            
            _parkLevelView.gameObject.SetActive(false);
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            
            _effectsAccumulatorView.FadeOut();

            _parkSprintController.Dispose();
            
            IsWin = win;
            
            OnMiniGameComplete?.Invoke(QType);
        }

        public void OnActionButtonClick()
        {
            
        }

        public void Dispose()
        {
            Object.Destroy(_parkLevelView);
            _parkSprintController?.Dispose();
            //TODO: Implement Dispose logic if needed
        }
    }
}