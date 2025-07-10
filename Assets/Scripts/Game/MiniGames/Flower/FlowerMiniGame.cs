using System;
using DG.Tweening;
using Game.Quests;
using Player;
using UI.Flower;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.MiniGames.Flower
{
    public class FlowerMiniGame : IMiniGame
    {
        private readonly IPlayerController _playerController;
        private readonly PressIndicator _pressIndicator;
        private readonly FlowerMiniGameView _flowerView;
        private GameObject _canView;
        private Vector3 _defaultPosition;
        private Quaternion _defaultRotation;
        private CanAnimatorTester _canAnimation;


        public bool IsCompleted { get; set; }
        public bool IsWin { get; private set; }
        public QuestType QType { get; } = QuestType.Flower;
        public int Level { get; set; } = 0;

        public Action<QuestType, bool> OnMiniGameComplete { get; set; }
        public event Action<QuestType> OnMiniGameStart;

        public FlowerMiniGame(IPlayerController playerController, MiniGamePrefabAccumulator prefabAccumulator,
            Canvas miniGameCanvas)
        {
            _playerController = playerController;

            Level = MiniGameCoordinator.DayLevel;
           
            _flowerView = Object.Instantiate(prefabAccumulator.FlowerMiniGameViews[Level], miniGameCanvas.transform);
            _pressIndicator = Object.Instantiate(prefabAccumulator.PressIndicator, _flowerView.PressPoint);
            
            _pressIndicator.gameObject.SetActive(false);
            _flowerView.gameObject.SetActive(false);
            
            _pressIndicator.OnCompleteIndicator += CompleteFlowerMiniGame;
            _pressIndicator.SetMultiplier(_flowerView.PressForce);
        }
        
        public void StartGame()
        {
            OnMiniGameStart?.Invoke(QType);
            
            _canView.gameObject.SetActive(true);
            
            StartCanAnimationAndStartPressInteraction();
           
            
            _flowerView.gameObject.SetActive(true);
        }

        private void StartCanAnimationAndStartPressInteraction()
        {
            _canAnimation.TogglePouring();
            
            RectTransform uiRect = _flowerView.CanPoint as RectTransform;
            Canvas canvas = uiRect.GetComponentInParent<Canvas>();
            Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCam, uiRect.position);

            float distanceZ = Mathf.Abs(Camera.main.transform.position.z - _canView.transform.position.z);
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(new Vector3(
                screenPoint.x,
                screenPoint.y,
                distanceZ
            ));
            
            //Собираем последовательность: движение → поворот → включение индикатора
            var seq = DOTween.Sequence();
            seq.Append(_canView.transform
                .DOMove(worldPoint, 1.5f)
                .SetEase(Ease.OutBack)
            );
            seq.Insert(0.2f, _canView.transform
                .DOLocalRotate(
                    new Vector3(
                        _canView.transform.localEulerAngles.x,
                        180f,
                        _canView.transform.localEulerAngles.z
                    ),
                    0.5f
                )
                .SetEase(Ease.InOutSine)
            );
            seq.OnComplete(() =>
            {
                _pressIndicator.gameObject.SetActive(true);
            });
        }

        private void CompleteFlowerMiniGame(bool isSuccess)
        {
            _pressIndicator.OnCompleteIndicator -= CompleteFlowerMiniGame;
            
            OnMiniGameComplete?.Invoke(QType, isSuccess);
            
            _pressIndicator.gameObject.SetActive(false);
            _flowerView.gameObject.SetActive(false);
            _canView.gameObject.SetActive(false);
            _canView.transform.position = _defaultPosition;
            _canView.transform.rotation = _defaultRotation;
            _canAnimation.TogglePouring();
            
            SetReward(isSuccess);
        }

        private void SetReward(bool isSuccess)
        {
            if (isSuccess)
            {
                _playerController.Model.Score += _flowerView.WinScore;
                IsWin = true;
            }
            else
            {
                _playerController.Model.Score -= 100;
                IsWin = false;
            }
        }

        public void OnActionButtonClick()
        {
            _pressIndicator.HandleInteract();
        }

        public void Dispose()
        {
            _pressIndicator.OnCompleteIndicator -= CompleteFlowerMiniGame;
  
            Object.Destroy(_flowerView);
            Object.Destroy(_pressIndicator);
        }

        public void SetWateringCanView(GameObject wateringCanView)
        {
            _canView = wateringCanView.gameObject.transform.parent.gameObject;
            
            _defaultPosition = wateringCanView.transform.position;
            _defaultRotation = wateringCanView.transform.rotation;
            
            _canAnimation = wateringCanView.GetComponent<CanAnimatorTester>();
        }
    }
}