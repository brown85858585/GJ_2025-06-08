
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private InputActionAsset inputActions;
    private Rigidbody rb;
    private InputAction moveAction;
    private Vector3 moveDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // Находим Action Map и Action
        InputActionMap playerMap = inputActions.FindActionMap("Player");
        moveAction = playerMap.FindAction("Move");
    }

    private void OnEnable() => moveAction?.Enable();
    private void OnDisable() => moveAction?.Disable();
    
    private void Update()
    {
        MoveActionRead();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }
    
    private void MoveActionRead() 
    {
        Vector2 move = moveAction.ReadValue<Vector2>();
        moveDirection = new Vector3(move.x, 0f, move.y).normalized;
    }
    
    private void MovePlayer()
    {
        Vector3 movement = moveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);
    }
}
