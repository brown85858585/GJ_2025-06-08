using System;
using Player.Interfaces;
using UnityEngine;

namespace Game.Interactions
{
    public class InteractionSystem : IInteractionSystem, IDisposable
    {
        public event Action<ItemCategory> OnInteraction;
        
        private readonly InteractionItemCollection _interactionItemCollection;
        private readonly IInputAdapter _inputAdapter;

        public InteractionSystem(InteractionItemCollection itemCollection, IInputAdapter inputAdapter)
        {
            _interactionItemCollection = itemCollection;
            foreach (var item in _interactionItemCollection.ObjectsToInteract)
            {
                item.OnEnter += SetItemInteractable;
                item.OnExit += RemoveItemInteractable;
            }
            
            _inputAdapter = inputAdapter;
            _inputAdapter.OnInteract += HandleInteract;
        }

        public void Dispose()
        {
            foreach (var item in _interactionItemCollection.ObjectsToInteract)
            {
                item.OnEnter -= SetItemInteractable;
                item.OnExit -= RemoveItemInteractable;
            }
            _inputAdapter.OnInteract -= HandleInteract;
        }

        private void RemoveItemInteractable(ItemInteractable item)
        {
            if (_interactionItemCollection.CurrentInteractable == item)
            {
                _interactionItemCollection.CurrentInteractable = null;

                foreach (var itemInCollection in _interactionItemCollection.ObjectsToInteract)
                {
                    itemInCollection.CheckStayCollider = true;
                }
            }
        }

        private void SetItemInteractable(ItemInteractable item)
        {
            _interactionItemCollection.CurrentInteractable?.TurnPopup(false);
            _interactionItemCollection.CurrentInteractable = item;
        }
        
        private void HandleInteract(bool interact)
        {
            if (interact && _interactionItemCollection.CurrentInteractable != null)
            {
                _interactionItemCollection.CurrentInteractable.Interact();
                OnInteraction?.Invoke(_interactionItemCollection.CurrentInteractable.Category);
                _interactionItemCollection.CurrentInteractable = null;
            }
        }
    }
}