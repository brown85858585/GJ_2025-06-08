using System;
using Game;
using Game.MiniGames;
using Player.Interfaces;
using UnityEngine;

namespace Player
{
    public class PlayerController : IPlayerController
    {
        public event Action OnDied;
        public IInputAdapter InputAdaptep => _input;
        private PlayerModel _model;
        private IInputAdapter _input;
        private PlayerView _view;
        Vector2 _moveInput;
        private bool _isRunMovement;

        private PlayerMovement _movement;
        private PlayerRunMovement _runMovement;
        private IPlayerMovement _currentMovement;
        public IPlayerMovement Movement => _currentMovement;
        public PlayerModel Model => _model;

        public PlayerController(PlayerModel model, IInputAdapter input, Transform camTransform)
        {
            _model = model;
            _input = input;

            _runMovement = new PlayerRunMovement(_input);
            _movement = new PlayerMovement(_input, model, camTransform);
            _currentMovement = _movement;
            
            _input.OnPutItemDown += PutTheItemDown;
            // _input.OnTest += ToggleMovement;
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
            SetPosition(Vector3.zero);
        }

        private void Update()
        {
            FixedUpdateMove();
        }

        private void OnCollision()
        {
            _currentMovement.SpeedDrop(_view.Rigidbody, _view.transform);
            DecreaseHealth();
        }

        public void FixedUpdateMove()
        {
            Model.CheckGrounded(_view.transform, _view.WhatIsGround);
            Model.ChangeGrid(_view.Rigidbody, _view.GroundDrag);

            var move = Movement.Move(_view.MoveSpeed, _view.transform);
            _view.SetWalkAnimation(move.normalized);
            _view.Rigidbody.AddForce(move, ForceMode.Force);
            
            var newRotation = Movement.Rotation(_view.transform, _view.TurnSmooth);
            _view.transform.rotation = newRotation;
        }

        public void SetPosition( Vector3 position)
        {
            _model.PlayerTransform.position = position;
        }

        public void ToggleMovement()
        {
            _isRunMovement = !_isRunMovement;
            if (_isRunMovement)
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
            // Debug.Log(_data.Stamina);
            if (_model.Stamina <= 0)
            {
                OnDied?.Invoke();
            }
        }

        public void HandleInteraction(ItemCategory item, Transform obj)
        {
            Debug.Log(item);
            if (item != ItemCategory.WateringCan)
            {
                if (item == ItemCategory.Flower)
                {
                    return;
                }

                PutTheItemDown();
                return;
            }
            
            _model.ItemInHand = ItemCategory.WateringCan;
            _view.TakeObject(obj);
            _view.StartDanceAnimation();
        }

        private void PutTheItemDown()
        {
            _model.ItemInHand = ItemCategory.None;
            _view.PutTheItemDown();
        }
    }


}