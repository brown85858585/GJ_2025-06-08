using System;
using Game;
using Game.MiniGames;
using Game.Models;
using Player.Interfaces;
using UnityEngine;

namespace Player
{
    public interface IPlayerController
    {
        public void SetPosition(Transform player, Vector3 position);
        public void FixedUpdateMove();
    }

    public class PlayerController : IPlayerController
    {
        public event Action OnDied;

        private PlayerModel _model;
        private IInputAdapter _input;
        private PlayerView _view;
        Vector2 _moveInput;
        private bool _testClicked;

        private PlayerMovement _movement;
        private PlayerRunMovement _runMovement;
        private IPlayerMovement _currentMovement;
        public IPlayerMovement Movement => _currentMovement;
        public PlayerModel Model => _model;

        [Header("Mini Game")]
        private MiniGameController _miniGameController;

        // private FlowerWateringGame _flowerWateringGame;

        public PlayerController(PlayerModel model, IInputAdapter input, Transform camTransform)
        {
            _model = model;
            _input = input;

            _runMovement = new PlayerRunMovement(_input);
            _movement = new PlayerMovement(_input, model, camTransform);
            _currentMovement = _movement;

            _input.OnTest += PutTheItemDown;

            // _flowerWateringGame = GameObject.FindObjectOfType<FlowerWateringGame>();
            // _input.OnTest += Testing;
        }

        public void InitView(PlayerView playerView)
        {
            _view = playerView;
            _view.OnButtonClick += OnButtonClick;
            _view.OnCollision += OnCollision;
            _view.OnUpdate += Update;
        }

        private void OnButtonClick()
        {
            SetPosition(_view.transform, Vector3.zero);
        }

        private void Update()
        {
            //todo deltaTimeFixedUpdate
            if (_miniGameController == null)
            {
                GameObject miniGameObj = GameObject.Find("MiniGameManager");
                if (miniGameObj != null)
                {
                    _miniGameController = miniGameObj.GetComponent<MiniGameController>();
                }
            }

            FixedUpdateMove();
        }

        private void OnCollision()
        {
            _currentMovement.SpeedDrop(_view.Rigidbody, _view.transform);
            DecreaseHealth();
        }

        public void FixedUpdateMove()
        {
            Model.CheckGrounded(_view.transform, _view.CapsuleCollider, _view.WhatIsGround);
            Model.ChangeGrid(_view.Rigidbody, _view.GroundDrag);

            var move = Movement.Move(_view.MoveSpeed, _view.transform);
            _view.SetWalkAnimation(move.magnitude);
            _view.Rigidbody.AddForce(move, ForceMode.Force);

            var newRotation = Movement.Rotation(_view.transform, _view.TurnSmooth);
            _view.transform.rotation = newRotation;
        }

        public void SetPosition(Transform player, Vector3 position)
        {
            player.position = position;
        }

        private void Testing(bool obj)
        {
            _testClicked = !_testClicked;
            if (_testClicked)
            {
                _currentMovement = _runMovement;
            }
            else
            {
                _currentMovement = _movement;
            }
        }

        private void DecreaseHealth()
        {
            _model.Stamina -= 20;
            Debug.Log(_model.Stamina);
            if (_model.Stamina <= 0)
            {
                OnDied?.Invoke();
            }
        }

        public void HandleInteraction(ItemCategory item, Transform obj)
        {
            /*
            if (_flowerWateringGame != null)
            {
                _flowerWateringGame.StartWateringSequence();
            }
            else
            {
                Debug.LogError("FlowerWateringGame не найден в сцене!");
            }
            */
            StartFlowerMiniGame();
            Debug.Log(item);
            if (item != ItemCategory.WateringCan) return;

            _model.ItemCategory = ItemCategory.WateringCan;
            _view.TakeObject(obj);
            _view.StartDanceAnimation();
        }

        private void PutTheItemDown(bool b)
        {
            _model.ItemCategory = ItemCategory.None;
            _view.PutTheItemDown();
        }
    



         private void StartFlowerMiniGame()
        {
            // Найти контроллер мини-игры в текущей сцене
            if (_miniGameController == null)
            {
                GameObject miniGameObj = GameObject.Find("MiniGameManager");
                if (miniGameObj != null)
                {
                    _miniGameController = miniGameObj.GetComponent<MiniGameController>();
                    //_miniGameController = FindObjectOfType<MiniGameController>();
                }
            }

            if (_miniGameController != null)
            {
                Debug.Log("✅ MiniGameController найден! Запуск мини-игры...");

                // Дополнительно убедиться что панель выключена перед запуском
                GameObject panel = GameObject.Find("MiniGamePanel");
                if (panel != null && panel.activeInHierarchy)
                {
                    Debug.Log("⚠️ Панель была включена, выключаем её перед запуском");
                    panel.SetActive(false);
                }

                // Подписаться на события мини-игры
                _miniGameController.OnMiniGameComplete += OnMiniGameCompleted;
                _miniGameController.OnWateringAttempt += OnWateringAttempt;

                // Запустить мини-игру (панель включится автоматически)
                _miniGameController.StartMiniGame();
            }
            else
            {
                Debug.LogError("❌ MiniGameController не найден! Проверь что скрипт добавлен на MiniGameManager в префабе комнаты.");
            }
        }

        // Колбэк когда мини-игра завершена
        private void OnMiniGameCompleted()
        {
            Debug.Log("Мини-игра завершена!");

            // Отписаться от событий чтобы избежать утечек памяти
            if (_miniGameController != null)
            {
                _miniGameController.OnMiniGameComplete -= OnMiniGameCompleted;
                _miniGameController.OnWateringAttempt -= OnWateringAttempt;
            }

            // Здесь можно добавить логику завершения:
            // - Дать награду игроку
            // - Показать анимацию роста цветка
            // - Обновить состояние цветка в комнате
            // - Сохранить прогресс

            Debug.Log("Взаимодействие с цветком завершено");
        }

        // Колбэк для каждой попытки полива
        private void OnWateringAttempt(bool success)
        {
            if (success)
            {
                Debug.Log("🌸 Успешный полив! Цветок доволен!");

                // Тут можно добавить:
                // - Звук успеха
                // - Партиклы воды
                // - Анимацию цветка
                // - Увеличить счетчик успешных поливов
            }
            else
            {
                Debug.Log("💧 Промах! Попробуй еще раз!");

                // Тут можно добавить:
                // - Звук промаха
                // - Анимация неудачи
                // - Feedback для игрока
            }
        }

        // Альтернативный метод если нужно вызвать мини-игру напрямую
        [ContextMenu("Test Mini Game")]
        public void TestMiniGame()
        {
            StartFlowerMiniGame();
        }

        // Метод для принудительного завершения мини-игры (если нужно)
        public void ForceEndMiniGame()
        {
            if (_miniGameController != null && _miniGameController.gameObject.activeSelf)
            {
                _miniGameController.EndMiniGame();
            }
        }
    }
}