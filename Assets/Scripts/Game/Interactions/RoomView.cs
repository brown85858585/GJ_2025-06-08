using System.Collections.Generic;
using UnityEngine;

namespace Game.Interactions
{
    public class RoomView : MonoBehaviour
    {
        [SerializeField] private Transform startPoint;
        [SerializeField] private List<ItemInteractable> objectsToInteract;

        public Transform StartPoint => startPoint;
        public List<ItemInteractable> ObjectsToInteract => objectsToInteract;

        private void OnDestroy()
        {
            objectsToInteract.Clear();
        }
    }
}