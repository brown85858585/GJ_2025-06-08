using Game;
using Game.Levels;
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
        private bool _isVectorRun;
        private LevelManager _levelManager;


        public PlayerModel Model => _model;
        public IPlayerDialogue Dialogue => _playerDialogue;

        private Vector3[] _targetPositions = {new Vector3(5.27f, 0f, 1.54f)};
        private float _vectorSpeed = 3f;

        public PlayerController(PlayerModel model, IInputAdapter input)
        {
            _model = model;
            _input = input;
            
            _movement = new PlayerMovement(_input, model);
            _currentMovement = _movement;
            
            _input.OnSwitchInteract += PutTheItemDown;
        }

        public void Init(PlayerView playerView, LevelManager levelManager)
        {
            _levelManager = levelManager;
            
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
            if(_levelManager.CurrentLevelIndex<6)
                _playerDialogue.OpenDialogue($"Day{_levelManager.CurrentLevelIndex + 1}_Awakening");
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
        public void ToggleMovement(bool isRunMovement)
        {
            _isRunMovement = isRunMovement;
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
            StopMovement(true);
        }

        public void StopMovement(bool isStop)
        {
            Debug.Log("Stop Movement: " + isStop);
            _stop = isStop;
        }
        
        public void ResetPlayer()
        {
            _normalSpeed = _view.MoveSpeed;
            _acceleratedSpeed = _view.RunSpeed;
            StopMovement(false);
            _view.SetExitAnimation();
            _view.ResetPlayerObj();
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
            
            if (_isVectorRun)
            {
                var rb = _view.Rigidbody;
                RigidbodyMover.MoveAlongPointsUpdate(
                    rb,
                    _targetPositions,
                     _vectorSpeed
                );
            }
            else if (Input.GetKey(KeyCode.Space))
            {
                var rb = _view.Rigidbody;
                RigidbodyMover.MoveAlongPointsUpdate(
                    rb,
                    new []{_targetPositions[0], new Vector3(13.5830784f,0f,0.0253740996f)},
                    _vectorSpeed
                );
            }
            else
            {
                Vector3 move;
                Quaternion newRotation;
                if (_input.IsAccelerating)
                {
                    move = Movement.Move(_acceleratedSpeed, _view.StaminaDecreaseMultiplayer);
                }
                else
                {
                    move = Movement.Move(_normalSpeed, _view.StaminaDecreaseMultiplayer);
                }
                _view.Rigidbody.AddForce(move, ForceMode.Force);
                newRotation = Movement.Rotation(_view.transform, _view.TurnSmooth);
                _view.transform.rotation = newRotation;
            }
            
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

        public void PlayWakeUpAnimation(float speed = 3f)
        {
            _view.SetWakeUpAnimation(speed);
        }

        private void PutTheItemDown()
        {
            _model.ItemInHand = ItemCategory.None;
            _view.PutTheItemDown();
        }

        public void StartVectorRun(float speed = 3, Vector3[] target = null)
        {
            Vector3 forceVector = new Vector3(30f, 0f, 20f);
// 1. Извлекаем направление
            Vector3 direction = forceVector.normalized;
// 2. Смещение на 100 метров
            Vector3 offset = direction * 1000f;
// 3. Целевая позиция
            var targetPos = _view.Rigidbody.position + offset;

            var endArr = new[] { targetPos };
            
            
            _isVectorRun = true;
            _targetPositions = target ?? endArr;
            _vectorSpeed = speed;
        }
        public void StopVectorRun()
        {
            _isVectorRun = false;
        }

        public void SwitchHead(int levelIndex)
        {
            switch (levelIndex)
            {
                case 0:
                case 1:
                    _view.EnableLongHair();
                    break;
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                    _view.EnableHat();
                    break;
                case 7:
                    _view.EnableShortHair();
                    break;
                default:
                    Debug.LogError("Unknown level index for head switch: " + levelIndex);
                    break;
            }
        }

        public void UpdateSpeed(int levelIndex)
        {
            switch (levelIndex)
            {
                case 0:
                case 1:
                    _view.ChangeSpeed(25f, 40f, 60f);
                    break;
                case 2:
                    _view.ChangeSpeed(25f, 35f, 55f);
                    break;
                case 3:
                    _view.ChangeSpeed(25f, 35f, 50f);
                    break;
                case 4: 
                    _view.ChangeSpeed(20f, 30f, 40f);
                    break;
                case 5:
                    _view.ChangeSpeed(20f, 25f, 35f);
                    break;
                case 6:
                case 7:
                    _view.ChangeSpeed();
                    break;
                default:
                    Debug.LogError("Unknown level index for head switch: " + levelIndex);
                    break;
            }
            
            _normalSpeed = _view.MoveSpeed;
            _acceleratedSpeed = _view.RunSpeed;
        }
    }
}