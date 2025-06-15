using System;
using Player.Interfaces;

namespace Game.Interactions
{
    public class InteractionSystem : IInteractionSystem, IDisposable
    {
        public event Action<ItemCategory> OnInteraction;
        
        private InteractionItemCollection _interactionItemCollection = new ();
        private readonly IInputAdapter _inputAdapter;
        public ItemInteractable CurrentInteractable { get; private set; }
        public InteractionSystem(IInputAdapter inputAdapter)
        {
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

        public void AddNewInteractionCollection(InteractionItemCollection itemCollection)
        {
            if (_interactionItemCollection.ObjectsToInteract != null)
            {
                foreach (var item in _interactionItemCollection.ObjectsToInteract)
                {
                    item.OnEnter -= SetItemInteractable;
                    item.OnExit -= RemoveItemInteractable;
                }
                _interactionItemCollection.ObjectsToInteract.Clear();
                
            }
            _interactionItemCollection = itemCollection;
           
            foreach (var item in _interactionItemCollection.ObjectsToInteract)
            {
                item.OnEnter += SetItemInteractable;
                item.OnExit += RemoveItemInteractable;
            }
            
        }
        
        private void RemoveItemInteractable(ItemInteractable item)
        {
            if (CurrentInteractable == item)
            {
                CurrentInteractable = null;

                foreach (var itemInCollection in _interactionItemCollection.ObjectsToInteract)
                {
                    itemInCollection.CheckStayCollider = true;
                }
            }
        }

        private void SetItemInteractable(ItemInteractable item)
        {
            CurrentInteractable?.TurnPopup(false);
            CurrentInteractable = item;
        }

        private void HandleInteract(bool interact)
        {
            if (interact && CurrentInteractable != null)
            {
                CurrentInteractable.Interact();
                OnInteraction?.Invoke(CurrentInteractable.Category);
                CurrentInteractable = null;
            }
        }
    }
}