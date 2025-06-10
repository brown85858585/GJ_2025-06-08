using System;
using Game;
using Game.Models;
using Player.Interfaces;
using UnityEngine;

namespace Player
{
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

        public PlayerController(PlayerModel model, IInputAdapter input, Transform camTransform)
        {
            _model = model;
            _input = input;

            _runMovement = new PlayerRunMovement(_input);
            _movement = new PlayerMovement(_input, model, camTransform);
            _currentMovement = _movement;
            
            _input.OnPutItemDown += PutTheItemDown;
            _input.OnTest += Testing;
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
            Debug.Log(item);
            if (item != ItemCategory.WateringCan) return;
            
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

    public interface IPlayerController
    {
        public void SetPosition(Transform player, Vector3 position);
        public void FixedUpdateMove();
    }
}