using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Button))]
    public class TMPButtonColor : MonoBehaviour, 
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        [Header("Ссылки")]
        public Image targetImage;                // фон
        public TextMeshProUGUI targetText;       // TMP-текст

        [Header("Цвета")]
        public Color normalImage = Color.white;
        public Color highlightedImage = Color.gray;
        public Color pressedImage = Color.black;
        public Color normalText = Color.black;
        public Color highlightedText = Color.white;
        public Color pressedText = Color.yellow;

        void Reset()
        {
            // автоподстановка, если повесили на кнопку
            var btn = GetComponent<Button>();
            targetImage = btn.targetGraphic as Image;
            targetText = GetComponentInChildren<TextMeshProUGUI>();
        }

        public void OnPointerEnter(PointerEventData e)
        {
            if (targetImage) targetImage.color = highlightedImage;
            if (targetText)  targetText.color  = highlightedText;
        }

        public void OnPointerExit(PointerEventData e)
        {
            if (targetImage) targetImage.color = normalImage;
            if (targetText)  targetText.color  = normalText;
        }

        public void OnPointerDown(PointerEventData e)
        {
            if (targetImage) targetImage.color = pressedImage;
            if (targetText)  targetText.color  = pressedText;
        }

        public void OnPointerUp(PointerEventData e)
        {
            // После отпускания снова в Highlighted, если курсор над кнопкой
            bool isHover = RectTransformUtility.RectangleContainsScreenPoint(
                transform as RectTransform, e.position, e.enterEventCamera);
            if (isHover) OnPointerEnter(e);
            else        OnPointerExit(e);
        }
    }
}