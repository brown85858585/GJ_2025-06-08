using System;
using Cysharp.Threading.Tasks;
using Effects.PostProcess;
using Game;
using Game.Installers;
using Game.MiniGames;
using Game.Quests;
using Player;
using Player.Interfaces;
using UnityEngine;
using Utilities;

namespace Scenario
{
    public class Day4Scenario : MonoBehaviour
    {
        [SerializeField] private Collider trigger;
        
        private MiniGameCoordinator _miniGameCoordinator;
        private PlayerController _playerController;
        private IInputAdapter _inputAdapter;
        private EffectAccumulatorView _effectAccumulatorView;
        
        private LayerMask _targetMask;
        private ScenarioInstaller _installer;

        private void Awake()
        {
            _targetMask = LayerMask.GetMask("Player");
            _installer = FindObjectOfType<ScenarioInstaller>();
            
        }

        private void Start()
        {
            Initialize(_installer.MiniGameCoordinator,
                _installer.PlayerController as PlayerController,
                _installer.InputAdapter,
                _installer.EffectAccumulatorView);
            
            _miniGameCoordinator.GetMiniGame(ItemCategory.Kitchen).OnMiniGameComplete += OnMiniGameComplete;
            
            trigger.gameObject.SetActive(false);
        }

        private void Initialize(MiniGameCoordinator miniGameCoordinator,
            PlayerController playerController,
            IInputAdapter inputAdapter,
            EffectAccumulatorView effectAccumulatorView)
        {
            _miniGameCoordinator = miniGameCoordinator;
            _playerController = playerController;
            _inputAdapter = inputAdapter;
            _effectAccumulatorView = effectAccumulatorView;
        }

        private void OnMiniGameComplete(QuestType arg1, bool arg2)
        {
            trigger.gameObject.SetActive(true);
            
            _inputAdapter.SwitchAdapterToMiniGameMode();
            Vector3[] arr = { new (5.27f, 0f, 1.54f), new (13.5830784f, 0f, 0.0253740996f) };
            _playerController.StartVectorRun(6f, arr);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (LayerChecker.CheckLayerMask(other.gameObject, _targetMask))
            {
                BathroomEffect().Forget();
            }
        }

        private async UniTask BathroomEffect()
        {
            _effectAccumulatorView.FadeOut(0.5f);
            await UniTask.Delay(1000);
            _playerController.SetPosition(new Vector3(13.5830784f, 0f, 0.0253740996f));
            _playerController.SetRotation(Quaternion.Euler(Vector3.left));
            _playerController.StopVectorRun();
            await UniTask.Delay(1000);
            // проиграть звук слива и блевания
            // дождаться завершения этого звука
            _effectAccumulatorView.FadeIn();
            _inputAdapter.SwitchAdapterToGlobalMode();
            trigger.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_miniGameCoordinator != null)
            {
                _miniGameCoordinator.GetMiniGame(ItemCategory.Kitchen).OnMiniGameComplete -= OnMiniGameComplete;
            }   
        }
    }
}