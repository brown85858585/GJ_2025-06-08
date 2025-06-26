using System.Collections.Generic;
using UI;
using UnityEngine;

namespace Game.Interactions
{
    public class RoomView : MonoBehaviour
    {
        [SerializeField] private Transform startPoint;
        [SerializeField] private List<ItemInteractable> objectsToInteract;
        [SerializeField] private FollowProjectionWithConstantSize popupForInteract;

        public Transform StartPoint => startPoint;
        public List<ItemInteractable> ObjectsToInteract => objectsToInteract;
        public FollowProjectionWithConstantSize PopupForInteract => popupForInteract;

        private void OnDestroy()
        {
            objectsToInteract.Clear();
        }
    }
}