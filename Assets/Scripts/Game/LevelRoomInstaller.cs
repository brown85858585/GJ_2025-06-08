using System;
using CameraField;
using Cinemachine;
using Game.Interactions;
using Game.MiniGames;
using Game.Models;
using Game.Quests;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Game
{
    public class LevelRoomInstaller :  MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private GameObject firstLevelPrefab;
        [SerializeField] private GameObject secondLevelPrefab;
        [SerializeField] private QuestLogView questLogPrefab;

        private PlayerModel _playerModel;
        private PlayerController _playerController;
        private InputAdapter _inputAdapter;
        private InteractionSystem _interactionSystem;
        private QuestsModel _questsModel;
        private QuestLog _questLog;
        private InteractionItemCollection _interactibles;
        private GameObject _firstLevel;
        private MiniGameCoordinator _miniGameCoordinator;

        private void Awake()
        {
            Install();
        }

        private void Install()
        {
            _questsModel = new QuestsModel();
            _playerModel = new PlayerModel(new DayModel());
            _inputAdapter = new InputAdapter(playerInput);
            _playerController = new PlayerController(_playerModel, _inputAdapter, virtualCamera.transform);
            
            _interactionSystem = new InteractionSystem(_inputAdapter);
            
            _playerController.OnDied += SecondLevel;
        }

        private void SecondLevel()
        {
            _playerController.OnDied -= SecondLevel;
            var go = Instantiate(secondLevelPrefab, transform);
               
            var secondInstaller = go.GetComponent<LevelSecondInstaller>();
            secondInstaller.Initialize(_playerController, _playerModel);
            
            Destroy(_firstLevel);
            _interactibles = go.GetComponentInChildren<InteractionItemCollection>();
            _interactionSystem.AddNewInteractionCollection(_interactibles);
        }

        void Start()
        {
            PlayerInit();

            CameraInit();
            
            _firstLevel =Instantiate(firstLevelPrefab, transform);
            
            _interactibles = _firstLevel.GetComponentInChildren<InteractionItemCollection>();
            _interactionSystem.AddNewInteractionCollection(_interactibles);
            
            _interactionSystem.OnInteraction += HandlePlayerInteraction;

            _miniGameCoordinator = new MiniGameCoordinator(_interactionSystem, _playerModel);
            
            QuestInit();
        }

        private void QuestInit()
        {
            var questsView = Instantiate(questLogPrefab, transform);
            _questLog = new QuestLog(questsView, _inputAdapter, _questsModel);
            
            _interactionSystem.OnInteraction += HandleQuestInteraction;
            
            foreach (var game in _miniGameCoordinator.Games)
            {
                game.OnMiniGameComplete += _questLog.CompleteQuest;
            }
        }

        private void HandleQuestInteraction(ItemCategory obj)
        {
            switch (obj)
            {
                case ItemCategory.None:
                    break;
                case ItemCategory.Bed:
                    _questLog.CompleteQuest(QuestType.Work);
                    break;
                case ItemCategory.Door:
                    _questLog.CompleteQuest(QuestType.Sprint);
                    break;
                case ItemCategory.Kitchen:
                    _questLog.CompleteQuest(QuestType.Kitchen);
                    break;
                case ItemCategory.Flower:
                case ItemCategory.WateringCan:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(obj), obj, null);
            }
        }

        private void HandlePlayerInteraction(ItemCategory item)
        {
            _playerController.HandleInteraction(item, _interactionSystem.CurrentInteractable.transform);
        }

        private void PlayerInit()
        {
            var go = Instantiate(playerPrefab);
            _playerModel.PlayerTransform = go.transform;
            
            var component = go.GetComponent<PlayerView>();
            _playerController.InitView(component);
        }

        private void CameraInit()
        {
            virtualCamera.Follow = _playerModel.PlayerTransform;
            virtualCamera.LookAt = _playerModel.PlayerTransform;

            var cameraRotation = virtualCamera.gameObject.AddComponent<CameraRotation>();
            cameraRotation.Initialization(_inputAdapter, virtualCamera.transform);
        }
    }
}