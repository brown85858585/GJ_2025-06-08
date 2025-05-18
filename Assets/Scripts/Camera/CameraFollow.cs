
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target; // Перетащите target в это поле в инспекторе
    [SerializeField] private float leadDistance = 2f; // Дистанция опережения
    [SerializeField] private float smoothSpeed = 5f; // Плавность движения
    private Vector3 offset; // Смещение камеры //  = new Vector3(0, 2, -5)???
    
    private Vector3 velocity = Vector3.zero;

    private void Awake ()
    {
        offset = transform.position;
    }

    private void FixedUpdate()
    {
        CameraFollowPlayer();
    }
    private void CameraFollowPlayer() 
    {
        if (target == null) return;

        // Рассчитываем точку опережения перед игроком
        Vector3 leadPosition = target.position + target.forward * leadDistance;
        
        // Целевая позиция камеры с учетом смещения
        Vector3 desiredPosition = leadPosition + offset;
        
        // Плавное перемещение
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothSpeed * Time.fixedDeltaTime
        );
    }
}