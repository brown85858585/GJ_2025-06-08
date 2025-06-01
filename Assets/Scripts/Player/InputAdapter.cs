using System;
using Player.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public sealed class InputAdapter : IDisposable, IInputAdapter
    {
        public event Action<Vector3> OnMove;
        public Vector3 Direction { get; private set; }
        public Vector3 Look { get; private set; }
        public bool IsAccelerating => _accelerateAction.IsPressed();

        private readonly InputAction _accelerateAction;
        private readonly InputAction _rotateAction;
        private readonly InputAction _lookAction;

        public InputAdapter(PlayerInput playerInput)
        {
            if (playerInput == null) throw new ArgumentNullException(nameof(playerInput));

            _accelerateAction = playerInput.actions.FindAction("Sprint", true);
            _rotateAction     = playerInput.actions.FindAction("Move",     true);
            _lookAction     = playerInput.actions.FindAction("Look",     true);

            _accelerateAction.Enable();
            _rotateAction.Enable();
            _lookAction.Enable();

            _rotateAction.performed += OnMoveInput;
            _rotateAction.canceled  += OnMoveInput;
            _lookAction.performed += OnLook;
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
            
            OnMove?.Invoke(Direction);
            Debug.Log(Direction);
        }

        public void Dispose()
        {
            _rotateAction.performed -= OnMoveInput;
            _rotateAction.canceled  -= OnMoveInput;
            
            _lookAction.performed -= OnLook;
            _lookAction.canceled -= OnLook;
            
            _accelerateAction.Disable();
            _rotateAction.Disable();
            _lookAction.Disable();
        }
    }
}