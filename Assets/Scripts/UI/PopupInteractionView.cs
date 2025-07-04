using System;
using System.Collections.Generic;
using Game;
using Game.Interactions;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PopupInteractionView : MonoBehaviour
    {
        [SerializeField] private Image popupImage;
        [SerializeField] private List<Sprite> sprites;

        public void SetInteractionImage(ItemCategory interactable)
        {
            
            Sprite chosenSprite;
            switch (interactable)
            {
                case ItemCategory.Bed:
                    chosenSprite = sprites[6];
                    break;
                case ItemCategory.Flower:
                    chosenSprite = sprites[4];
                    break;
                case ItemCategory.Kitchen:
                    chosenSprite = sprites[1];
                    break;
                case ItemCategory.WateringCan:
                    chosenSprite = sprites[3];
                    break;
                case ItemCategory.Computer:
                    chosenSprite = sprites[2];
                    break;
                case ItemCategory.Door:
                    chosenSprite = sprites[0];
                    break;
                case ItemCategory.Books:
                case ItemCategory.None:
                case ItemCategory.Guitar:
                case ItemCategory.Painting:
                case ItemCategory.Basket:
                case ItemCategory.Window:
                default:
                    chosenSprite = sprites[5];
                    break;
            }
            
            popupImage.sprite = chosenSprite;

            // Вариант 1. Устанавливает размер точно под исходные пиксели спрайта:
            popupImage.SetNativeSize();
        }
    }
}