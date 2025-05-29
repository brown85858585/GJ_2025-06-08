using System;
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
        private IInputAdapter _input;
        InputAction _moveAction;
        Vector2 _moveInput;
        
       
        public PlayerController(PlayerModel model, IInputAdapter input)
        {
            _model = model;
            _input = input;
        }

        public void SetPosition(Vector3 position)
        {
            _view.TransformPlayer.position = position;
        }
        // Явная привязка View
        public void Initialize(IPlayerView view)
        {
            _view = view;
            _view.OnCollision += DecreaseHealth;
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

        public void Update()
        {
            var dir = _input.Direction;
            _model.MoveDirection = dir;
            _view.Move(dir);
        }
    }

    public interface IPlayerView
    {
        Transform TransformPlayer { get; set; }
        public event Action OnCollision;
        void Move(Vector3 direction);
    }

}