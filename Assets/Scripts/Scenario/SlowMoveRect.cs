using UnityEngine;

public class SlowMoveRect : MonoBehaviour
{
    [Tooltip("Ссылка на RectTransform, который будем двигать")]
    [SerializeField] private RectTransform rectTransform;

    [Tooltip("Скорость движения в единицах UI-координат в секунду")]
    [SerializeField] private float speed = 100f;

    [Tooltip("Конечная Y-координата (anchoredPosition.y), куда нужно добраться")]
    [SerializeField] private float targetY = 500f;

    private void Reset()
    {
        // Попытка автоматически найти RectTransform на том же объекте
        rectTransform = GetComponent<RectTransform>();
    }

    private void Awake()
    {
        if (rectTransform == null)
        {
            Debug.LogError("RectTransform не назначен!", this);
            enabled = false;
        }
    }

    private void Update()
    {
        Vector2 pos = rectTransform.anchoredPosition;

        // Пока не достигли целевой Y-координаты — двигаем вверх
        if (pos.y < targetY)
        {
            pos.y += speed * Time.deltaTime;
            // Чтобы не «перескочить» чуть выше targetY
            if (pos.y > targetY) pos.y = targetY;
            rectTransform.anchoredPosition = pos;
        }
    }
}