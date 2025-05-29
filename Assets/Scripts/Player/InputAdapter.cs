using System;
using Player.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public sealed class InputAdapter : IDisposable, IInputAdapter
    {
        public Vector3 Direction { get; private set; }
        public bool IsAccelerating => _accelerateAction.IsPressed();

        private readonly InputAction _accelerateAction;
        private readonly InputAction _rotateAction;

        public InputAdapter(PlayerInput playerInput)
        {
            if (playerInput == null) throw new ArgumentNullException(nameof(playerInput));

            _accelerateAction = playerInput.actions.FindAction("Sprint", true);
            _rotateAction     = playerInput.actions.FindAction("Move",     true);

            _accelerateAction.Enable();
            _rotateAction.Enable();

            _rotateAction.performed += OnMove;
            _rotateAction.canceled  += OnMove;
        }

        private void OnMove(InputAction.CallbackContext callbackContext)
        {
            var readValue = callbackContext.ReadValue<Vector2>();
            Direction = new Vector3(readValue.x, 0f, readValue.y);
            if (callbackContext.canceled) Direction = Vector2.zero;
        }

        public void Dispose()
        {
            _rotateAction.performed -= OnMove;
            _rotateAction.canceled  -= OnMove;

            _accelerateAction.Disable();
            _rotateAction.Disable();
        }
    }
}