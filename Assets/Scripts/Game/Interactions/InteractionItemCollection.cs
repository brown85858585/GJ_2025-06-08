using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Interactions
{
    public class InteractionItemCollection : MonoBehaviour
    {
        [SerializeField] private List<ItemInteractable> objectsToInteract;

        public List<ItemInteractable> ObjectsToInteract => objectsToInteract;

        private void OnDestroy()
        {
            objectsToInteract.Clear();
        }
    }
}