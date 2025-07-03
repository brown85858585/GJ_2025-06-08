using System;
using Cysharp.Threading.Tasks;
using Game;
using Player.Interfaces;
using UnityEngine;
using Utilities;

namespace Player
{
    public class PlayerController : IPlayerController
    {
        public Transform CamTransform
        {
            set => _movement.VirtualCamera = value;
        }

        public IInputAdapter InputAdaptep => _input;
        private PlayerModel _model;
        private IInputAdapter _input;
        private PlayerView _view;
        Vector2 _moveInput;
        private bool _isRunMovement;

        private PlayerMovement _movement;
        private IPlayerMovement _currentMovement;
        private PlayerDialogue _playerDialogue;
        public IPlayerMovement Movement => _currentMovement;
        
        private float _normalSpeed;
        private float _acceleratedSpeed;
        private bool _stop;

        public void ToggleMovement()
        {
            _isRunMovement = !_isRunMovement;
            if (_isRunMovement)
            {
                _normalSpeed = _view.RunSpeed;
                _acceleratedSpeed = _view.SprintSpeed;
                
                // _movement.VirtualCamera.
            }
            else
            {
                _normalSpeed = _view.MoveSpeed;
                _acceleratedSpeed = _view.RunSpeed;
            }
        }

        public void SetFallingAnimation()
        {
            Debug.Log("Falling");
            _view.SetTriggerFalling();
            _stop = true;
        }

        public PlayerModel Model => _model;
        public IPlayerDialogue Dialogue => _playerDialogue;

        public PlayerController(PlayerModel model, IInputAdapter input)
        {
            _model = model;
            _input = input;
            
            _movement = new PlayerMovement(_input, model);
            _currentMovement = _movement;
            
            _input.OnSwitchInteract += PutTheItemDown;
        }

        public void InitView(PlayerView playerView)
        {
            _view = playerView;
            _normalSpeed = _view.MoveSpeed;
            _acceleratedSpeed = _view.RunSpeed;
            
            _view.OnCollision += OnCollision;
            _view.OnUpdate += Update;
            
            _playerDialogue = new PlayerDialogue(_view.DialogueView);
            
            
            //todo change normal switcher 
            //Input off
            _input.SwitchAdapterToMiniGameMode();
            _view.OnWakeUpEnded += InputOn;
        }

        private void InputOn()
        {
            _input.SwitchAdapterToGlobalMode();
        }

        private void Update()
        {
            FixedUpdateMove();
        }

        private void OnCollision(Collision collision)
        {
            if (!LayerChecker.CheckLayerMask(collision.gameObject, _view.WhatIsGround))
            {
                _currentMovement.SpeedDrop(_view.Rigidbody, _view.transform);
            }
        }

        public void ClampSpeed()
        {
             _normalSpeed = 18f;
             _acceleratedSpeed = 19f;
        }

        private void FixedUpdateMove()
        {
            if(_stop) return;
            Model.CheckGrounded(_view.transform, _view.WhatIsGround);
            Model.ChangeGrid(_view.Rigidbody, _view.GroundDrag);

            Vector3 move;
            if (_input.IsAccelerating)
            {
                move = Movement.Move(_acceleratedSpeed, _view.StaminaDecreaseMultiplayer);
            }
            else
            {
                move = Movement.Move(_normalSpeed, _view.StaminaDecreaseMultiplayer);
            }
            _view.Rigidbody.AddForce(move, ForceMode.Force);
            var newRotation = Movement.Rotation(_view.transform, _view.TurnSmooth);
            _view.transform.rotation = newRotation;
            
            _view.SetWalkAnimation(_input.Direction.normalized);
        }

        public void SetPosition( Vector3 position)
        {
            _view.Rigidbody.MovePosition(position);
        }
        public void SetRotation(Quaternion rotation)
        {
            _view.transform.rotation = rotation;
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
        }

        public void PlayWakeUpAnimation()
        {
            _view.SetWakeUpAnimation();
        }

        private void PutTheItemDown()
        {
            _model.ItemInHand = ItemCategory.None;
            _view.PutTheItemDown();
        }
    }
}