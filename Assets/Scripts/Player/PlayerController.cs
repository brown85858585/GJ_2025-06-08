using System;
using Cinemachine;
using Game.Models;
using Player.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerController
    {
        public event Action OnDied;

        private PlayerModel _model;
        private IPlayerView _view;
        private IPlayerMovement _movement;
        private IInputAdapter _input;
        InputAction _moveAction;
        Vector2 _moveInput;
        private bool _testClicked;
        private Vector3 _direction;
        private readonly Transform _virtualCamera;


        public PlayerController(PlayerModel model, IInputAdapter input, Transform camTransform)
        {
            _model = model;
            _input = input;
            _virtualCamera = camTransform;
        }

        public void SetPosition(Vector3 position)
        {
            _view.TransformPlayer.position = position;
        }

        // Явная привязка View
        public void Initialize(IPlayerView view)
        {
            _view = view;
            _movement = view.Movement;
            _view.OnCollision += DecreaseHealth;

            _input.OnTest += Testing;


            _movement.CameraTransform = _virtualCamera;
        }

        private void Testing(bool obj)
        {
            _testClicked = !_testClicked;
            if (_testClicked)
            {
                _view.SetRotateMovement();
                _movement = _view.Movement;
            }
            else
            {
                _view.SetNormalMovement();
                _movement = _view.Movement;
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

        public void FixedUpdate()
        {
            var dir = _input.Direction;
            _model.MoveDirection = dir;
            
            _movement.Move(_model.MoveDirection);
        }
    }
}