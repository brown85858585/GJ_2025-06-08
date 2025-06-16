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
using UnityEngine.Serialization;

namespace Game
{
    public class LevelRoomInstaller :  MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject virtualCameraObj;
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
        private RoomView _roomView;
        private GameObject _firstLevel;
        private MiniGameCoordinator _miniGameCoordinator;
        private bool _allQuestCompleted;

        private void Awake()
        {
            Install();
        }

        private void Install()
        {
            _questsModel = new QuestsModel();
            _playerModel = new PlayerModel(new DayModel());
            
            _inputAdapter = new InputAdapter(playerInput);
            _interactionSystem = new InteractionSystem(_inputAdapter);
            
            _playerController = new PlayerController(_playerModel, _inputAdapter, virtualCameraObj.transform);
            _miniGameCoordinator = new MiniGameCoordinator(_interactionSystem, _playerModel, _playerController);
        }

        private void SecondLevel()
        {
            var go = Instantiate(secondLevelPrefab, transform);
               
            var secondInstaller = go.GetComponent<LevelSecondInstaller>();
            secondInstaller.Initialize(_playerController, _playerModel);
            
            Destroy(_firstLevel);
            _roomView = go.GetComponentInChildren<RoomView>();
            _interactionSystem.AddNewInteractionCollection(_roomView);
        }

        void Start()
        {
            _firstLevel =Instantiate(firstLevelPrefab, transform);
            _roomView = _firstLevel.GetComponentInChildren<RoomView>();
            
            PlayerInit();

            CameraInit();
            
            _interactionSystem.AddNewInteractionCollection(_roomView);
            
            _interactionSystem.OnInteraction += HandlePlayerInteraction;

            _miniGameCoordinator.RegisterGames(_firstLevel);
            
            QuestInit();
        }

        private void QuestInit()
        {
            var questsView = Instantiate(questLogPrefab, transform);
            _questLog = new QuestLog(questsView, _inputAdapter, _questsModel);
            
            _interactionSystem.OnInteraction += HandleCompleteLevelInteraction;
            
            foreach (var game in _miniGameCoordinator.Games)
            {
                game.OnMiniGameComplete += _questLog.CompleteQuest;
            }
            
            _questLog.AllQuestsCompleted += () =>
            {
                _allQuestCompleted = true;
            };
        }

        private void HandleCompleteLevelInteraction(ItemCategory obj)
        {
            if(obj == ItemCategory.Bed && _allQuestCompleted)
            {
                SecondLevel();
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
            _playerController.SetPosition(_roomView.StartPoint.position);
        }

        private void CameraInit()
        {
            var virtualCamera = Instantiate(virtualCameraObj, transform.parent).GetComponent<CinemachineVirtualCamera>();
            
            virtualCamera.Follow = _playerModel.PlayerTransform;
            virtualCamera.LookAt = _playerModel.PlayerTransform;

            var cameraRotation = virtualCamera.gameObject.AddComponent<CameraRotation>();
            cameraRotation.Initialization(_inputAdapter, virtualCamera.transform);
        }
    }
}