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

        public event Action<bool> OnTest;
        
        private readonly InputAction _accelerateAction;
        private readonly InputAction _moveAction;
        private readonly InputAction _lookAction;
        private readonly InputAction _testAction;

        public InputAdapter(PlayerInput playerInput)
        {
            if (playerInput == null) throw new ArgumentNullException(nameof(playerInput));

            _accelerateAction = playerInput.actions.FindAction("Sprint", true);
            _moveAction     = playerInput.actions.FindAction("Move",     true);
            _lookAction     = playerInput.actions.FindAction("Look",     true);
            _testAction     = playerInput.actions.FindAction("Test",     true);

            _accelerateAction.Enable();
            _moveAction.Enable();
            _lookAction.Enable();
            _testAction.Enable();

            _moveAction.performed += OnMoveInput;
            _moveAction.canceled  += OnMoveInput;
            _lookAction.performed += OnLook;
            _lookAction.canceled += OnLook;
            
            _testAction.started += OnTestInput;
        }

        private void OnTestInput(InputAction.CallbackContext obj)
        {
            var readValue = obj.ReadValue<float>();
            OnTest?.Invoke(readValue != 0);
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
            
            _accelerateAction.Disable();
            _moveAction.Disable();
            _lookAction.Disable();
        }
    }
}