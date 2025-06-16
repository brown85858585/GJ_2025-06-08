using System;
using Player.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public sealed class InputAdapter : IDisposable, IInputAdapter
    {
        public Vector3 Direction { get; private set; }
        public Vector3 Look { get; private set; }
        public bool IsAccelerating => _accelerateAction.IsPressed();
        public event Action<bool> OnInteract;
        public event Action OnPutItemDown;
        public event Action<bool> OnTest;
        public event Action<bool> OnQuests;
        
        private readonly InputAction _accelerateAction;
        private readonly InputAction _moveAction;
        private readonly InputAction _lookAction;
        private readonly InputAction _testAction;
        private readonly InputAction _interactAction;
        private readonly InputAction _crouchAction;
        private readonly InputAction _questsAction;
        
        private readonly InputAction _NavigateAction;

        public InputAdapter(PlayerInput playerInput)
        {
            if (playerInput == null) throw new ArgumentNullException(nameof(playerInput));
            
            _accelerateAction = playerInput.actions.FindAction("Sprint", true);
            _moveAction     = playerInput.actions.FindAction("Move",     true);
            _lookAction     = playerInput.actions.FindAction("Look",     true);
            _testAction     = playerInput.actions.FindAction("Test",     true);
            _interactAction = playerInput.actions.FindAction("Interact", true);
            _crouchAction = playerInput.actions.FindAction("Crouch", true);
            _questsAction = playerInput.actions.FindAction("Quests", true);
            _NavigateAction = playerInput.actions.FindAction("Navigate", true);
        
            
            _accelerateAction.Enable();
            _moveAction.Enable();
            _lookAction.Enable();
            
            _testAction.Enable();
            _interactAction.Enable();
            _crouchAction.Enable();
            _NavigateAction.Enable();
            _NavigateAction.performed += context =>
            {
               Debug.Log("Navigate " + context.action.name);
            };
            _moveAction.performed += OnMoveInput;
            _moveAction.canceled  += OnMoveInput;
            _lookAction.performed += OnLook;
            _lookAction.canceled += OnLook;
            
            _testAction.started += OnTestInput;
            _testAction.canceled += OnTestInput;
            
            _questsAction.started += OnQuestsInput;
            _questsAction.canceled += OnQuestsInput;
            
            _interactAction.started += OnInteractInput;
            _crouchAction.started += OnPutItemDownInput;
          
            playerInput.actions.FindActionMap("UI", true).Disable();

            
        }

        private void OnPutItemDownInput(InputAction.CallbackContext obj)
        {
            OnPutItemDown?.Invoke();
        }

        private void OnInteractInput(InputAction.CallbackContext obj)
        {
            var readValue = obj.ReadValue<float>();
            OnInteract?.Invoke(readValue != 0);
            if (obj.canceled) OnInteract?.Invoke(false);
        }

        private void OnTestInput(InputAction.CallbackContext obj)
        {
            var readValue = obj.ReadValue<float>();
            OnTest?.Invoke(readValue != 0);
        }
        
        private void OnQuestsInput(InputAction.CallbackContext obj)
        {
            var readValue = obj.ReadValue<float>();
            OnQuests?.Invoke(readValue != 0);
        }


        private void OnLook(InputAction.CallbackContext obj)
        {
            var readValue = obj.ReadValue<Vector2>();
            Look = new Vector3(readValue.x, 0f, readValue.y);
            if (obj.canceled) Look = Vector2.zero;
        }

        private void OnMoveInput(InputAction.CallbackContext callbackContext)
        {
            var readValue = callbackContext.ReadValue<Vector2>();
            Direction = new Vector3(readValue.x, 0f, readValue.y);
            if (callbackContext.canceled) Direction = Vector2.zero;
        }

        public void Dispose()
        {
            _moveAction.performed -= OnMoveInput;
            _moveAction.canceled  -= OnMoveInput;
            
            _lookAction.performed -= OnLook;
            _lookAction.canceled -= OnLook;
            _interactAction.started -= OnInteractInput;
            _testAction.started -= OnTestInput;
            
            _accelerateAction.Disable();
            _moveAction.Disable();
            _lookAction.Disable();
            _interactAction.Disable();
            _testAction.Disable();
        }
    }
}