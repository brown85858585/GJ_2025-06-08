
using UnityEngine;

public class RotateTowardsMovement : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 3f;
    
    [SerializeField] private Rigidbody rb;
    private Vector3 lastMoveDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        RotationOnDirection();
    }
    private void RotationOnDirection()
    {
        Vector3 moveDirection = rb.velocity.normalized;
        
        if (moveDirection != Vector3.zero)
        {
            lastMoveDirection = moveDirection;
            Quaternion targetRotation = Quaternion.LookRotation(lastMoveDirection);

            // Quaternion rotationCorrection = Quaternion.Euler(0, -90f, 0);
            // targetRotation *= rotationCorrection;

            rb.rotation = Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
        }
    }
}